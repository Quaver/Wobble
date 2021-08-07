# Wobble [![Build Status](https://travis-ci.com/Quaver/Wobble.svg?branch=master)](https://travis-ci.com/Quaver/Wobble) [![CodeFactor](https://www.codefactor.io/repository/github/quaver/wobble/badge/master)](https://www.codefactor.io/repository/github/quaver/wobble/overview/master) [![Trello](https://img.shields.io/badge/Trello-Roadmap-blue.svg)](https://trello.com/b/QVbVwKN1/quaver-client) [![Discord](https://discordapp.com/api/guilds/354206121386573824/widget.png?style=shield)](https://discord.gg/nJa8VFr)

Wobble is a powerful and bare-bones extension of the [MonoGame Framework](https://github.com/MonoGame/MonoGame) designed to make the initial boilerplate process of developing games so much simpler. 

This framework allows you to follow a familiar and easy-to-learn paradigm while still giving you full control of MonoGame itself.

The original purpose of Wobble was to provide an underlying and organized framework for [Quaver](https://github.com/Quaver), a competitive-oriented and open-source rhythm game currently in development, however it can be used for just about any game. 

If you can master this framework, you'll have no problem jumping in on the Quaver development which is encouraged.

# Requirements

* [.NET Core SDK 3.1](https://www.microsoft.com/net/download)

# Getting Started

Wobble is designed to work directly with the [MonoGame.Framework.DesktopGL Nuget Package](https://www.nuget.org/packages/MonoGame.Framework.DesktopGL/). It has not been tested with the others, although it should work properly.

If creating a new game, it's best to start with that, as Wobble provides all the dlls needed to get up and running.

Currently there is no NuGet package for Wobble, however this may change in the future.

## Steps

These are the following steps that we find to be particularly handy when using Wobble with new projects.

**1.** Create a new C# .NET Core project.  

**2.** Clone Wobble and add it as a submodule to your project.

**3.** Run `git submodule update --init --recursive` to install all of Wobble's dependencies.

**4.** Reference both Wobble and MonoGame.Framework.DesktopGL.Core in your project.

**5.** Create a class that derives from `WobbleGame`. In this case, we'll call it `MyGame`. It should look similar to this:

```cs
public class MyGame : WobbleGame
{
    /// <inheritdoc />
    /// <summary>
    /// </summary>
    protected override bool IsReadyToUpdate { get; set; }

    /// <inheritdoc />
    /// <summary>
    ///     Allows the game to perform any initialization it needs to before starting to run.
    ///     This is where it can query for any required services and load any non-graphic
    ///     related content.  Calling base.Initialize will enumerate through any components
    ///     and initialize them as well.
    /// </summary>
    protected override void Initialize()
    {
        base.Initialize();
        
        // TODO: Your initialization code goes here.
    }

    /// <inheritdoc />
    /// <summary>
    ///     LoadContent will be called once per game and is the place to load
    ///     all of your content.
    /// </summary>
    protected override void LoadContent()
    {
        base.LoadContent();

        // TODO: Your asset loading code goes here.

        // Tell the game that it should start updating now
        IsReadyToUpdate = true;

        // TODO: Change to the first screen via ScreenManager
        // ScreenManager.ChangeScreen(new MainMenuScreen()); - Example
    }

    /// <inheritdoc />
    /// <summary>
    ///     UnloadContent will be called once per game and is the place to unload
    ///     game-specific content.
    /// </summary>
    protected override void UnloadContent()
    {
        base.UnloadContent();

        // TODO: Your disposing logic goes here.
    }

    /// <inheritdoc />
    /// <summary>
    ///     Allows the game to run logic such as updating the world,
    ///     checking for collisions, gathering input, and playing audio.
    /// </summary>
    protected override void Update(GameTime gameTime)
    {
        if (!IsReadyToUpdate)
            return;

        base.Update(gameTime);

        // TODO: Your global update logic goes here. Anything updated here will be updated on every screen.
    }

    /// <inheritdoc />
    /// <summary>
    ///     This is called when the game should draw itself.
    /// </summary>
    protected override void Draw(GameTime gameTime)
    {
        if (!IsReadyToUpdate)
            return;

        base.Draw(gameTime);

        // TODO: Your global draw logic goes here. Anything drawn here will be drawn on top of every screen.
    }
}
```

**6.** The **main method** *(usually in Program.cs)* of your application should look very straight forward. 

```cs
internal static class Program
{
    [STAThread]
    internal static void Main(string[] args)
    {
        using (var game = new MyGame())
        {
            game.Run();
        }
    }
}
```

**7.** Start building screens!

**8.** Profit?

## Creating Screens

As stated in our goals, the core focus of Wobble is to skip all of the initial boilerplate and set up code you'd usually do with MonoGame/XNA and go straight to developing your screens.

There are five major components to take notice of when developing screens: 

* **ScreenManager** - An under the hood static class that controls the addition, changing, removal, updating, and drawing of screens.
* **Screen** - A portion of your game used to update and render a single state or scene of your game. You might want to have a main menu screen, and options screen, and a gameplay screen for example.
* **ScreenView** - Each screen has one of these. It's where all of your GUI code should be placed. The initializing, updating, and drawing of sprites should live here. Think about it like the view in the MVC architecture. No business logic, just display.
* **Container** - Every ScreenView has a container. In short, it serves a parent to the majority of the sprites on the screen. 

    Each child depends on their parent for their positioning and size. As an example, if you were to align a container at the mid left of the screen, and a child sprite at the top right, the position of the sprite would be at the top right of the container, at the mid left of the screen.
* **Sprite/SpriteText** - It's pretty self explanatory, but these are the sprites that are actually drawn to the screen. A sprite must have a parent of another `Drawable` (Container, another Sprite/SpriteText)

As long as you understand that hierarchy, you should have no problem creating screens.

## Demos

There a a bunch of demos and tests that you can use to learn how Wobble works. 

You should first check out the example [GreenBox](https://github.com/Swan/Wobble/tree/master/Examples/GreenBox) game. A simple game that draws a green box to the screen and moves it around with our awesome transformation system.

There are also tons of [visual tests](https://github.com/Swan/Wobble/tree/master/Wobble.Tests) you can run and take a look at to see how each part of Wobble can be used.

# Contributing 
We love and encourage people to contribute to Quaver - whether it be through code, designs, or ideas. Our mission is to provide a space where the rhythm game community's voice is able to be heard, which we feel has been neglected in the past. We aim to create the ultimate mania rhythm game, and this isn't possible without community input.

The best place to begin contributing to Quaver is through our [Discord server](https://discord.gg/nJa8VFr), where all of our developers, community, and testers are located.

Any contributions can be made through opening a pull request. When contributing however, please keep in mind that there should only be one branch/pull request per feature. This means if your branch is titled `my-new-feature`, you shouldn't have any unrelated commits, and the same system is applied to pull requests.

If you are wanting to develop a feature with the goal of having it being in the Steam release, open up an issue first, so we can discuss if it is in the scope of what we are trying to accomplish. If it is accepted, it'll be added to our official [Trello board](https://trello.com/b/QVbVwKN1/quaver-client). 

When contributing, please remember to follow our [code style](https://github.com/Quaver/Wobble/blob/master/CODESTYLE.md), so the codebase is consistent across the board. If you have any issues with our approaches to structuring/styling our code, feel free to bring this up.

# Special Thanks

The creation of this framework couldn't have been possible without the following people/organizations.

**People**

* [Staravia](https://github.com/Staravia) - For building out the base of the Drawable & Sprite system.
* [Swan](https://github.com/Swan) - For putting the majority of this framework together.
* [Vortex-](https://github.com/VortexCoyote) - For developing the included shaders.
* [Lachee](https://github.com/Lachee/discord-rpc-csharp) - For their C# Discord Rich Presence Library

**Orgs**

* [MonoGame](https://github.com/MonoGame/MonoGame) - The awesome framework this is built on top of.
* [ManagedBass](https://github.com/ManagedBass/ManagedBass) - For their .NET wrapper for the [BASS Audio Library](http://www.un4seen.com/)

# License

Wobble is released and licensed under [The MIT License (MIT)](https://github.com/Swan/Wobble/blob/master/LICENSE).

**Please Note:** [BASS](http://www.un4seen.com/), the audio library dependency of this framework, is free to use for non-commercial purposes. If you plan on using this framework for a commercial game, be sure to purchase a license.
