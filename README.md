# Skybox AI Generator by Blockade Labs

Create stunning AI-generated skybox assets within Unity for use as HDRIs in game dev and immersive projects.

## Prerequisites
In order to use this package you need to provide an API key from Blockade Labs in the API section.
Get one at <a href="https://api.blockadelabs.com" target="_blank">https://api.blockadelabs.com</a>.

## Installation

Option 1: Unity Asset Store
- Add this package to your assets from the Unity Asset Store.
- Go to `Window > Package Manager`
- Change the project scope to `My Assets`
- Find `Blockade Labs SDK` and click `Install`.

Option 2: git package
- Go to `Window > Package Manager`
- `+ > Add package from git URL...`
- Enter `https://github.com/Blockade-Games/BlockadeLabs-SDK-Unity.git`

## Changelog
Refer to the changelog file [here](CHANGELOG.md).

## Unity Versions Support

2020.3 and newer.

## Getting Started

After importing the package, open `Packages > Blockade Labs SDK > Scenes > SkyboxScene`.

**Note**: Demo scene uses Text Mesh Pro elements for runtime UI. If you haven't imported TMP Essentials
you will be prompted to do so after you load the scene. When you are done importing TMP Essentials,
reload the scene by either right clicking on it in the `Assets/Samples/Blockade Labs SDK/x.x.x/Scenes`
folder and selecting `Reimport` or by loading some other scene and then loading the Skybox Scene again.

## Generating Skyboxes in Edit Mode

You will now see a gameObjects named `Skybox Sphere`.
The Sphere has a Mesh Renderer component which uses a sample `Skybox Material` which in turn uses a
shader of type `Skybox/Panoramic`. A texture generated with this package is assigned to the shader.

You can generate a new texture that will replace the existing one on the sphere by following these steps.

1. Select the Skybox Sphere gameObject.
2. Locate the `Blockade Labs Skybox` component.
3. Add your Blockade Labs' `API key` in the designated field.
4. Click the `Apply` button.
5. After the plugin is successfully initialized, some additional fields will become available.
6. You'll notice that the object has an option `Assign to Material` ticked. Leave it as it is.
7. Select the desired style.
8. Fill in the `Prompt` field.
9. Click the `Generate Skybox` Button.
10. In about 20-30 seconds your texture will be replaced with a new texture you just created, and a folder located in `Assets/Blockade Labs SDK` will now hold your newly created texture.
11. You should see your new skybox in the game view. You can also click `Move Scene Camera to Skybox` to see the skybox in the scene view.

## Generating Skyboxes at Runtime

To be able to generate assets on runtime you just need to follow these simple steps:

1. Select the Skybox Sphere gameObject.
2. Locate the `Blockade Labs Skybox` component.
3. Add your Blockade Labs' `API key` in the designated field.
4. The SkyboxScene already has the necessary elements to display the UI for Skybox generation on runtime.
5. After you run the game UI will become active on top of your Game view.
6. Use the runtime UI in the same manner as you would in the editor (`Enter Prompt > Select a style > Generate`).