# Changelog

## 1.6.1 - December 21, 2023.

- Added Mesh Creator mode, which uses a new shader to display the depth map.
- Blockade Labs Skybox is split into two components:
  - `Blockade Labs Skybox Generator` generates the textures and material.
  - `Blockade Labs Skybox` generates a mesh and configures the shader for Mesh Creator mode.
  - See README for details.

## 1.6.0 - December 11, 2023.

- Reorganize package to more standard package format.
- Overhaul the runtime UI to match the web interface.
- Add 'remix' support.
- Add "Move Scene Camera to Skybox" button to Blockade Labs Skybox component.
- Improve stability and error reporting.

## 1.5.4 - October 11th, 2023.

- Fix the issue with depth maps for trial users

## 1.5.0 - September 14th, 2023.

- Generate depth maps along with original skyboxes
- Add enhance prompt param
- Compress textures

## 1.4.0 - May 11th, 2023.

- Update editor and runtime UI
- Add negative text and seed params

## 1.3.0 - April 3rd, 2023.

- Switch to using obfuscated IDs to track generations
- Minor fixes

## 1.2.0 - March 30th, 2023.

- Initial public release

## 1.0.0/1.1.0 - Pre-March 2023.

- Pre-release builds and tests