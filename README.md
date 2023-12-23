# Skybox AI Generator by Blockade Labs

Create stunning AI-generated skybox assets within Unity for use as HDRIs in game dev and immersive projects.

## Prerequisites
In order to use this package you need to provide an API key from Blockade Labs in the API section.
Get one at <a href="https://api.blockadelabs.com" target="_blank">https://api.blockadelabs.com</a>.

## Installation

Option 1: Install from the Unity Asset Store
- Add this package to your assets from the Unity Asset Store.
- Go to `Window > Package Manager`
- Change the project scope to `My Assets`
- Find `Blockade Labs SDK` and click `Install`.

Option 2: Install with OpenUPM-CLI:
- `openupm add com.blockadelabs.sdk`

Option 3: Install as a git package
- Go to `Window > Package Manager`
- `+ > Add package from git URL...`
- Enter `https://github.com/Blockade-Games/BlockadeLabs-SDK-Unity.git`

### Pusher Package
The Blockade SDK can be used standalone or optionally together with a Pusher websockets package. If installed, the Pusher package will use websockets to listen for any changes in the Asset Generation Process on Runtime and make updates accordingly, which should improve performance for your games on Runtime.

You can learn more about the Pusher package [here](https://github.com/pusher/pusher-websocket-unity).

The Pusher library requires .NET Framework runtime:
- Set `Edit > Project Settings > Player > Api Compatibility Level` to `.NET Framework`.

Option 1: Install the Pusher package with git:
- Go to `Window > Package Manager`
- `+ > Add package from git URL...`
- Enter `https://github.com/pusher/pusher-websocket-unity.git#upm`

Option 2: Install the Pusher package with OpenUPM-CLI:
- `openupm add com.pusher.pusherwebsocketunity`

After installing the Pusher package on 2021.x.x versions you might get an error saying:

`Assembly 'Packages/com.pusher.pusherwebsocketunity/Packages/PusherClient.2.1.0/lib/net472/PusherClient.dll' will not be loaded due to errors: PusherClient references strong named Newtonsoft.Json Assembly references: 12.0.0.0 Found in project: 13.0.0.0.`

To resolve the issue go to `Edit > Project Settings > Player > Other Settings > Configuration > Assembly Version Validation` and disable `Version Validation`.

## Changelog
Refer to the changelog file [here](CHANGELOG.md).

## Unity Versions Support

2020.3 and newer.

## Getting Started

After importing the package, open `Packages > Blockade Labs SDK > Scenes > SkyboxScene`.

**Note**: Demo scene uses Text Mesh Pro elements for runtime UI. If you haven't imported TMP Essentials
you will be prompted to do so after you load the scene. When you are done importing TMP Essentials,
reload the scene by either double clicking on it in the `Assets/Samples/Blockade Labs SDK/Scenes`.

## Generating Skyboxes in Edit Mode

The scene contains two notable gameObjects:
- `Blockade Labs Skybox Generator` generates skybox textures and materials.
- `Blockade Labs Skybox` generates a mesh to display the skybox.

You can generate a new skybox that will replace the existing one by following these steps.

1. Select the `Blockade Labs Skybox Generator` gameObject.
2. Add your Blockade Labs' `API key` in the designated inspector field.
3. Click the `Apply` button.
4. After the plugin is successfully initialized, some additional fields will become available.
5. Select the desired style.
6. Fill in the `Prompt` field.
7. Click the `Generate Skybox` Button.
8. In about 20-30 seconds a new material will be generated and assigned to the `Blockade Labs Skybox` gameObject. You will also find a folder located in `Assets/Blockade Labs SDK` that holds the generated textures and materials.
9. You should see your new skybox in the game view. You can also click `Move Scene Camera to Skybox` to see the skybox in the scene view.

## Generating Skyboxes at Runtime

To be able to generate assets on runtime you just need to follow these simple steps:

1. Select the `Blockade Labs Skybox Generator` gameObject.
2. Add your Blockade Labs' `API key` in the designated field.
3. Play the scene. You will see the necessary elements to generate a new skybox. You can look around by clicking and dragging the mouse and zoom with the scroll wheel.
5. Use the runtime UI in the same manner as you would in the editor: enter a prompt, select a style, click generate.

## Mesh Creator

The `Blockade Labs Skybox Generator` component generates a color texture and a depth texture, which are assigned to the skybox material.

The `Blockade Labs Skybox` component generates a Tetrahedron mesh to apply this material. You can configure the `Mesh Density` and `Depth Scale` fields. The mesh and material will be configured to apply the generated depth map to deform the mesh.

Try zooming in and out with the scroll wheel in play mode to see the effect of the depth scale!