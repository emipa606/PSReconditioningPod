# GitHub Copilot Instructions for [PS] Reconditioning Pod (Continued)

## Mod Overview and Purpose

The [PS] Reconditioning Pod (Continued) mod for RimWorld introduces a novel way to manage pawns' traits through advanced plasteel-based reconditioning technology. It offers players the ability to modify, add, or remove traits from colonist characters, effectively altering their behavior and interactions within the game. Enabling players to refine their colony's dynamics, this mod updates the original by Neon1028, adding new functionalities and enhancing performance.

## Key Features and Systems

- **Trait Management**: Modify colonist traits via a reconditioning pod with options to add, remove, or swap traits.
- **Pod Biometrics**: Each pod biometrically assigns a specific colonist to prevent unauthorized use.
- **Conditioning and Maintenance**: Conditioning effect degrades 12.5% per day; maintenance is required via pods or Conditionall, a portable drug lab product.
- **Neural Cementing**: Permanently cements desired traits, though it's a risky procedure.
- **Trait Value and Searchable UI**: Enhanced UI elements allow for searching and sorting traits, supporting the integration of Trait Values.

## Coding Patterns and Conventions

- **Class Structure**: The mod employs structured classes for different functionalities, adhering to a single-responsibility principle.
- **Method Naming**: Follows a clear verb-noun naming pattern to increase readability (e.g., `StartReconditioning`, `TryAssignPawn`).
- **Private/Internal Members**: Applied encapsulation using private/internal access modifiers to limit class and method accessibility.

## XML Integration

- The mod uses XML to define various game objects, such as traits, buildings, and drugs. XML files are essential for defining trait properties and in-game interactions.
- Ensure XML definitions are up-to-date to match added features or changes, particularly when integrating new traits or modifying existing ones.

## Harmony Patching

- **Patching Practices**: Uses Harmony for modifying game behavior without altering core files, ensuring compatibility with other mods.
- **Patch Locations**: Focuses on methods dealing with trait alteration, pod interaction, and need adjustments.
- Ensure patches are concise and specifically target methods to minimize unexpected behaviors and bugs.

## Suggestions for Copilot

- **Code Completion**: Assist in writing methods that handle trait modification logic, involving complex condition checks and UI updates.
- **XML Suggestions**: Guide in writing new XML definitions or modifications for traits and buildings, ensuring syntax correctness.
- **Harmony Patch Proposals**: Propose Harmony patches for new features, maintaining consistency with existing game logic.
- **Bug Detection**: Provide insights for potential issues, especially in complex method implementations like those managing conditioning sequences.
- **UI Enhancements**: Support UI-related code improvements, enhancing the sorting and searching capabilities for traits within the mod interface.

This instruction file serves as a guide for utilizing GitHub Copilot efficiently within the [PS] Reconditioning Pod (Continued) project. By adhering to the described patterns and considerations, contributions to the mod will be both effective and coherent.


This markdown file provides an overview and guidance for using GitHub Copilot in developing and maintaining the provided RimWorld mod. It covers the mod's purpose, its systems and features, coding conventions, XML usage, Harmony patching, and suggestions for leveraging Copilot effectively.
