﻿using System;
using Wobble.Discord.RPC.Exceptions;
using Wobble.Discord.RPC.IO;
using Wobble.Discord.RPC.Logging;
using Wobble.Discord.RPC.Message;
using Wobble.Discord.RPC.Registry;
using Wobble.Discord.RPC.RPC;
using Wobble.Discord.RPC.RPC.Commands;

namespace Wobble.Discord.RPC
{
	/// <summary>
	/// A Discord RPC Client which is used to send Rich Presence updates and receive Join / Spectate events.
	/// </summary>
	public class DiscordRpcClient : IDisposable
	{
		#region Properties
		/// <summary>
		/// Gets a value indicating if the RPC Client has registered a URI scheme. If this is false, Join / Spectate events will fail.
		/// </summary>
		public bool HasRegisteredUriScheme { get; private set; }

		/// <summary>
		/// Gets the Application ID of the RPC Client.
		/// </summary>
		public string ApplicationID { get; private set; }

		/// <summary>
		/// Gets the Steam ID of the RPC Client. This value can be null if none was supplied.
		/// </summary>
		public string SteamID { get; private set; }

		/// <summary>
		/// Gets the ID of the process used to run the RPC Client. Discord tracks this process ID and waits for its termination.
		/// </summary>
		public int ProcessID { get; private set; }

		/// <summary>
		/// The dispose state of the client object.
		/// </summary>
		public bool Disposed => _disposed;

		private bool _disposed = false;

		/// <summary>
		/// The logger used this client and its associated components. <see cref="ILogger"/> are not called safely and can come from any thread. It is upto the <see cref="ILogger"/> to account for this and apply appropriate thread safe methods.
		/// </summary>
		public ILogger Logger
		{
			get => _logger;
			set
			{
				this._logger = value;
				if (connection != null) connection.Logger = value;
			}
		}
		private ILogger _logger = new NullLogger();
		#endregion

		/// <summary>
		/// The pipe the discord client is on, ranging from 0 to 9. Use -1 to scan through all pipes.
		/// <para>This property can be used for testing multiple clients. For example, if a Discord Client was on pipe 0, the Discord Canary is most likely on pipe 1.</para>
		/// </summary>
		public int TargetPipe => _pipe;

		private int _pipe = -1;
		private RpcConnection connection;

		/// <summary>
		/// The current presence that the client has. Gets set with <see cref="SetPresence(RichPresence)"/> and updated on <see cref="OnPresenceUpdate"/>.
		/// </summary>
		public RichPresence CurrentPresence => _presence;

		private RichPresence _presence;

		/// <summary>
		/// Current subscription to events. Gets set with <see cref="Subscribe(EventType)"/>, <see cref="UnsubscribeMessage"/> and updated on <see cref="OnSubscribe"/>, <see cref="OnUnsubscribe"/>.
		/// </summary>
		public EventType Subscription => _subscription;

		private EventType _subscription;

		/// <summary>
		/// The current discord user. This is updated with the ready event and will be null until the event is fired from the connection.
		/// </summary>
		public User CurrentUser => _user;

		private User _user;

		/// <summary>
		/// The current configuration the connection is using. Only becomes available after a ready event.
		/// </summary>
		public Configuration Configuration => _configuration;

		private Configuration _configuration;

		/// <summary>
		/// Represents if the client has been <see cref="Initialize"/>
		/// </summary>
		public bool IsInitialized => _initialized;

		private bool _initialized;

		/// <summary>
		/// Forces the connection to shutdown gracefully instead of just aborting the connection.
		/// <para>This option helps prevents ghosting in applications where the Process ID is a host and the game is executed within the host (ie: the Unity3D editor). This will tell Discord that we have no presence and we are closing the connection manually, instead of waiting for the process to terminate.</para>
		/// </summary>
		public bool ShutdownOnly
		{
			get => _shutdownOnly;
			set { _shutdownOnly = value; if (connection != null) connection.ShutdownOnly = value; }
		}
		private bool _shutdownOnly = true;

		#region Events

		/// <summary>
		/// Called when the discord client is ready to send and receive messages.
		/// <para>This event is not invoked untill <see cref="Invoke"/> is executed.</para>
		/// </summary>
		public event OnReadyEvent OnReady;

		/// <summary>
		/// Called when connection to the Discord Client is lost. The connection will remain close and unready to accept messages until the Ready event is called again.
		/// <para>This event is not invoked untill <see cref="Invoke"/> is executed.</para>
		/// </summary>
		public event OnCloseEvent OnClose;

		/// <summary>
		/// Called when a error has occured during the transmission of a message. For example, if a bad Rich Presence payload is sent, this event will be called explaining what went wrong.
		/// <para>This event is not invoked untill <see cref="Invoke"/> is executed.</para>
		/// </summary>
		public event OnErrorEvent OnError;

		/// <summary>
		/// Called when the Discord Client has updated the presence.
		/// <para>This event is not invoked untill <see cref="Invoke"/> is executed.</para>
		/// </summary>
		public event OnPresenceUpdateEvent OnPresenceUpdate;

		/// <summary>
		/// Called when the Discord Client has subscribed to an event.
		/// <para>This event is not invoked untill <see cref="Invoke"/> is executed.</para>
		/// </summary>
		public event OnSubscribeEvent OnSubscribe;

		/// <summary>
		/// Called when the Discord Client has unsubscribed from an event.
		/// <para>This event is not invoked untill <see cref="Invoke"/> is executed.</para>
		/// </summary>
		public event OnUnsubscribeEvent OnUnsubscribe;

		/// <summary>
		/// Called when the Discord Client wishes for this process to join a game.
		/// <para>This event is not invoked untill <see cref="Invoke"/> is executed.</para>
		/// </summary>
		public event OnJoinEvent OnJoin;

		/// <summary>
		/// Called when the Discord Client wishes for this process to spectate a game.
		/// <para>This event is not invoked untill <see cref="Invoke"/> is executed.</para>
		/// </summary>
		public event OnSpectateEvent OnSpectate;

		/// <summary>
		/// Called when another discord user requests permission to join this game.
		/// <para>This event is not invoked untill <see cref="Invoke"/> is executed.</para>
		/// </summary>
		public event OnJoinRequestedEvent OnJoinRequested;

		/// <summary>
		/// The connection to the discord client was succesfull. This is called before <see cref="MessageType.Ready"/>.
		/// <para>This event is not invoked untill <see cref="Invoke"/> is executed.</para>
		/// </summary>
		public event OnConnectionEstablishedEvent OnConnectionEstablished;

		/// <summary>
		/// Failed to establish any connection with discord. Discord is potentially not running?
		/// </summary>
		public event OnConnectionFailedEvent OnConnectionFailed;
		#endregion

		#region Initialization

		/// <summary>
		/// Creates a new Discord RPC Client without using any uri scheme. This will disable the Join / Spectate functionality.
		/// </summary>
		/// <param name="applicationID"></param>
		public DiscordRpcClient(string applicationID) : this(applicationID, -1) { }

		/// <summary>
		/// Creates a new Discord RPC Client without using any uri scheme. This will disable the Join / Spectate functionality.
		/// </summary>
		/// <param name="applicationID"></param>
		/// <param name="pipe">The pipe to connect too. -1 for first available pipe.</param>
		public DiscordRpcClient(string applicationID, int pipe) : this(applicationID, null, false, pipe) { }


		/// <summary>
		/// Creates a new Discord RPC Client using the default uri scheme.
		/// </summary>
		/// <param name="applicationID">The ID of the application created at discord's developers portal.</param>
		/// <param name="registerUriScheme">Should a URI scheme be registered for Join / Spectate functionality? If false, the Join / Spectate functionality will be disabled.</param>
		public DiscordRpcClient(string applicationID, bool registerUriScheme) : this(applicationID, registerUriScheme, -1) { }

		/// <summary>
		/// Creates a new Discord RPC Client using the default uri scheme.
		/// </summary>
		/// <param name="applicationID">The ID of the application created at discord's developers portal.</param>
		/// <param name="registerUriScheme">Should a URI scheme be registered for Join / Spectate functionality? If false, the Join / Spectate functionality will be disabled.</param>
		/// <param name="pipe">The pipe to connect too. -1 for first available pipe.</param>
		public DiscordRpcClient(string applicationID, bool registerUriScheme, int pipe) : this(applicationID, null, registerUriScheme, pipe) { }

		/// <summary>
		/// Creates a new Discord RPC Client using the steam uri scheme.
		/// </summary>
		/// <param name="applicationID">The ID of the application created at discord's developers portal.</param>
		/// <param name="steamID">The steam ID of the app. This is used to launch Join / Spectate through steam URI scheme instead of manual launching</param>
		/// <param name="pipe">The pipe to connect too. -1 for first available pipe.</param>
		public DiscordRpcClient(string applicationID, string steamID, bool registerUriScheme) : this(applicationID, steamID, registerUriScheme, -1) { }

		/// <summary>
		/// Creates a new Discord RPC Client using the steam uri scheme.
		/// </summary>
		/// <param name="applicationID">The ID of the application created at discord's developers portal.</param>
		/// <param name="steamID">The steam ID of the app. This is used to launch Join / Spectate through steam URI scheme instead of manual launching</param>
		/// <param name="registerUriScheme">Should a URI scheme be registered for Join / Spectate functionality? If false, the Join / Spectate functionality will be disabled.</param>
		/// <param name="pipe">The pipe to connect too. -1 for first available pipe.</param>
		public DiscordRpcClient(string applicationID, string steamID, bool registerUriScheme, int pipe) : this(applicationID, steamID, registerUriScheme, pipe, new ManagedNamedPipeClient()) { }

		/// <summary>
		/// Creates a new Discord RPC Client using the steam uri scheme.
		/// </summary>
		/// <param name="applicationID">The ID of the application created at discord's developers portal.</param>
		/// <param name="steamID">The steam ID of the app. This is used to launch Join / Spectate through steam URI scheme instead of manual launching</param>
		/// <param name="registerUriScheme">Should a URI scheme be registered for Join / Spectate functionality? If false, the Join / Spectate functionality will be disabled.</param>
		/// <param name="pipe">The pipe to connect too. -1 for first available pipe.</param>
		/// <param name="client">The pipe client to use and communicate to discord through</param>
		public DiscordRpcClient(string applicationID, string steamID, bool registerUriScheme, int pipe, INamedPipeClient client)
		{

			//Store our values
			ApplicationID = applicationID;
			SteamID = steamID;
			HasRegisteredUriScheme = registerUriScheme;
			ProcessID = System.Diagnostics.Process.GetCurrentProcess().Id;
			_pipe = pipe;

			//Create the RPC client
			connection = new RpcConnection(ApplicationID, ProcessID, TargetPipe, client) { ShutdownOnly = _shutdownOnly };
			connection.Logger = this._logger;
		}

		#endregion

		#region Message Handling
		/// <summary>
		/// Dequeues all the messages from Discord and invokes appropriate methods. This will process the message and update the internal state before invoking the events. Returns the messages that were invoked and in the order they were invoked.
		/// </summary>
		/// <returns>Returns the messages that were invoked and in the order they were invoked.</returns>
		public IMessage[] Invoke()
		{
			//Dequeue all the messages and process them
			var messages = connection.DequeueMessages();
			for (int i = 0; i < messages.Length; i++)
			{
				//Do a bit of pre-processing
				var message = messages[i];
				HandleMessage(message);

				//Invoke the appropriate methods
				switch (message.Type)
				{
					case MessageType.Ready:
						if (OnReady != null) OnReady.Invoke(this, message as ReadyMessage);
						break;

					case MessageType.Close:
						if (OnClose != null) OnClose.Invoke(this, message as CloseMessage);
						break;

					case MessageType.Error:
						if (OnError != null) OnError.Invoke(this, message as ErrorMessage);
						break;

					case MessageType.PresenceUpdate:
						if (OnPresenceUpdate != null) OnPresenceUpdate.Invoke(this, message as PresenceMessage);
						break;

					case MessageType.Subscribe:
						if (OnSubscribe != null) OnSubscribe.Invoke(this, message as SubscribeMessage);
						break;

					case MessageType.Unsubscribe:
						if (OnUnsubscribe != null) OnUnsubscribe.Invoke(this, message as UnsubscribeMessage);
						break;

					case MessageType.Join:
						if (OnJoin != null) OnJoin.Invoke(this, message as JoinMessage);
						break;

					case MessageType.Spectate:
						if (OnSpectate != null) OnSpectate.Invoke(this, message as SpectateMessage);
						break;

					case MessageType.JoinRequest:
						if (OnJoinRequested != null) OnJoinRequested.Invoke(this, message as JoinRequestMessage);
						break;

					case MessageType.ConnectionEstablished:
						if (OnConnectionEstablished != null) OnConnectionEstablished.Invoke(this, message as ConnectionEstablishedMessage);
						break;

					case MessageType.ConnectionFailed:
						if (OnConnectionFailed != null) OnConnectionFailed.Invoke(this, message as ConnectionFailedMessage);
						break;

					default:
						//This in theory can never happen, but its a good idea as a reminder to update this part of the library if any new messages are implemented.
						Logger.Error("Message was queued with no appropriate handle! {0}", message.Type);
						break;
				}
			}

			//Finally, return the messages
			return messages;
		}

		/// <summary>
		/// Gets a single message from the queue. This may return null if none are availble. This will process the message and update internal state before handing it over.
		/// </summary>
		/// <returns></returns>
		public IMessage Dequeue()
		{
			if (Disposed)
				throw new ObjectDisposedException("Discord IPC Client");

			//Dequeue the message and do some preprocessing
			IMessage message = connection.DequeueMessage();
			HandleMessage(message);

			//return the message
			return message;
		}

		/// <summary>
		/// Dequeues all messages from the Discord queue. This will be a empty array of size 0 if none are availble. This will process the messages and update internal state before handing it over.
		/// </summary>
		/// <returns></returns>
		public IMessage[] DequeueAll()
		{
			if (Disposed)
				throw new ObjectDisposedException("Discord IPC Client");

			//Dequeue all the messages and process them
			IMessage[] messages = connection.DequeueMessages();
			for (int i = 0; i < messages.Length; i++) HandleMessage(messages[i]);

			//Return it
			return messages;
		}

		private void HandleMessage(IMessage message)
		{
			//if (Disposed)
			//	throw new ObjectDisposedException("Discord IPC Client");

			if (message == null) return;
			switch (message.Type)
			{
				//We got a update, so we will update our current presence
				case MessageType.PresenceUpdate:
					var pm = message as PresenceMessage;
					if (pm != null)
					{
						//We need to merge these presences together
						if (_presence == null)
						{
							_presence = pm.Presence;
						}
						else if (pm.Presence == null)
						{
							_presence = null;
						}
						else
						{
							_presence.Merge(pm.Presence);
						}

						//Update the message
						pm.Presence = _presence;
					}

					break;

				//Update our configuration
				case MessageType.Ready:
					var rm = message as ReadyMessage;
					if (rm != null)
					{
						_configuration = rm.Configuration;
						_user = rm.User;

						//Resend our presence and subscription
						SynchronizeState();
					}
					break;

				//Update the request's CDN for the avatar helpers
				case MessageType.JoinRequest:
					if (Configuration != null)
					{
						//Update the User object within the join request if the current Cdn
						var jrm = message as JoinRequestMessage;
						if (jrm != null) jrm.User.SetConfiguration(Configuration);
					}
					break;

				case MessageType.Subscribe:
					var sub = message as SubscribeMessage;
					_subscription |= sub.Event;
					break;

				case MessageType.Unsubscribe:
					var unsub = message as UnsubscribeMessage;
					_subscription &= ~unsub.Event;
					break;

				//We got a message we dont know what to do with.
				default:
					break;
			}
		}
		#endregion

		/// <summary>
		/// Respond to a Join Request. Give TRUE to allow the user to join, otherwise false. All requests will timeout after 30 seconds, so be sure to <see cref="Dequeue"/> frequently enough.
		/// </summary>
		/// <param name="request">The request that is being responded too.</param>
		/// <param name="acceptRequest">Is the request accepted?</param>
		public void Respond(JoinRequestMessage request, bool acceptRequest)
		{
			if (Disposed)
				throw new ObjectDisposedException("Discord IPC Client");

			if (connection == null)
				throw new ObjectDisposedException("Connection", "Cannot initialize as the connection has been deinitialized");

			connection.EnqueueCommand(new RespondCommand() { Accept = acceptRequest, UserID = request.User.ID.ToString() });
		}

		/// <summary>
		/// Sets the Rich Presences
		/// </summary>
		/// <param name="presence">The rich presence to send to discord</param>
		public void SetPresence(RichPresence presence)
		{
			if (Disposed)
				throw new ObjectDisposedException("Discord IPC Client");

			if (connection == null)
				throw new ObjectDisposedException("Connection", "Cannot initialize as the connection has been deinitialized");

			//Update our internal store of the presence
			_presence = presence;
			if (!_presence)
			{
				//Clear the presence
				connection.EnqueueCommand(new PresenceCommand() { PID = this.ProcessID, Presence = null });
			}
			else
			{
				//Send valid presence
				//Validate the presence with our settings
				if (presence.HasSecrets())
				{
					if (!HasRegisteredUriScheme)
						throw new BadPresenceException("Cannot send a presence with secrets as this object has not registered a URI scheme!");

					if (!string.IsNullOrEmpty(presence.Secrets.JoinSecret) && !presence.HasParty())
						throw new BadPresenceException("Presences that include Join Secrets must also include a party!");
				}

				if (presence.HasParty() && presence.Party.Max < presence.Party.Size)
					throw new BadPresenceException("Presence maximum party size cannot be smaller than the current size.");

				//Send the presence
				connection.EnqueueCommand(new PresenceCommand() { PID = this.ProcessID, Presence = presence.Clone() });
			}
		}

		/// <summary>
		/// Clears the Rich Presence. Use this just before disposal to prevent ghosting.
		/// </summary>
		public void ClearPresence()
		{
			if (Disposed)
				throw new ObjectDisposedException("Discord IPC Client");

			if (connection == null)
				throw new ObjectDisposedException("Connection", "Cannot initialize as the connection has been deinitialized");

			//Just a wrapper function for sending null
			SetPresence(null);
		}

		/// <summary>
		/// Subscribes to an event from the server. Used for Join / Spectate feature. If you have not registered your application, this feature is unavailable.
		/// </summary>
		/// <param name="type"></param>
		public void Subscribe(EventType type) { SetSubscription(_subscription | type); }

		/// <summary>
		/// Subscribes to an event from the server. Used for Join / Spectate feature. If you have not registered your application, this feature is unavailable.
		/// </summary>
		/// <param name="type"></param>
		public void Unubscribe(EventType type) { SetSubscription(_subscription & ~type); }

		/// <summary>
		/// Sets the subscription flag, unsubscribing and then subscribing to the nessary events
		/// </summary>
		/// <param name="type">The events to subscribe too</param>
		public void SetSubscription(EventType type)
		{
			//Calculate what needs to be unsubscrinbed
			SubscribeToTypes(_subscription & ~type, true);
			SubscribeToTypes(~_subscription & type, false);
			_subscription = type;
		}

		/// <summary>
		/// Simple helper function that will subscribe to the specified types in the flag.
		/// </summary>
		/// <param name="type">The flag to subscribe to</param>
		/// <param name="isUnsubscribe">Represents if the unsubscribe payload should be sent instead.</param>
		private void SubscribeToTypes(EventType type, bool isUnsubscribe)
		{
			//Because of SetSubscription, this can actually be none as there is no differences.
			//If that is the case, we should just stop here
			if (type == EventType.None) return;

			//We cannot do anything if we are disposed or missing our connection.
			if (Disposed)
				throw new ObjectDisposedException("Discord IPC Client");

			if (connection == null)
				throw new ObjectDisposedException("Connection", "Cannot initialize as the connection has been deinitialized");

			//We dont have the Uri Scheme registered, we should throw a exception to tell the user.
			if (!HasRegisteredUriScheme)
				throw new InvalidConfigurationException("Cannot subscribe/unsubscribe to an event as this application has not registered a URI scheme.");

			//Add the subscribe command to be sent when the connection is able too
			if ((type & EventType.Spectate) == EventType.Spectate)
				connection.EnqueueCommand(new SubscribeCommand() { Event = RPC.Payload.ServerEvent.ActivitySpectate, IsUnsubscribe = isUnsubscribe });

			if ((type & EventType.Join) == EventType.Join)
				connection.EnqueueCommand(new SubscribeCommand() { Event = RPC.Payload.ServerEvent.ActivityJoin, IsUnsubscribe = isUnsubscribe });

			if ((type & EventType.JoinRequest) == EventType.JoinRequest)
				connection.EnqueueCommand(new SubscribeCommand() { Event = RPC.Payload.ServerEvent.ActivityJoinRequest, IsUnsubscribe = isUnsubscribe });
		}

		/// <summary>
		/// Resends the current presence and subscription. This is used when Ready is called to keep the current state within discord.
		/// </summary>
		public void SynchronizeState()
		{
			//Set the presence
			SetPresence(_presence);

			//Set the subscription
			SubscribeToTypes(_subscription, false);
		}

		/// <summary>
		/// Attempts to initalize a connection to the Discord IPC.
		/// </summary>
		/// <returns></returns>
		public bool Initialize()
		{
			if (Disposed)
				throw new ObjectDisposedException("Discord IPC Client");

			if (connection == null)
				throw new ObjectDisposedException("Connection", "Cannot initialize as the connection has been deinitialized");

			return _initialized = connection.AttemptConnection();
		}

		/// <summary>
		/// Terminates the connection to Discord and disposes of the object.
		/// </summary>
		public void Dispose()
		{
			if (Disposed) return;

			connection.Close();
			_disposed = true;
		}

	}
}
