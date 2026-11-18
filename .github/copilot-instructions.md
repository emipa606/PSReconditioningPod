# GitHub Copilot Instructions for [PS] Reconditioning Pod (Continued)

## Mod Overview and Purpose

[PS] Reconditioning Pod (Continued) is an update and expansion to Neon1028's original mod, introducing advanced mind-altering technology through the Plasteel Surgery Reconditioning Pod. This mod allows players to modify colonists' traits through a series of conditioning sessions. The pod can add, remove, or change traits, providing a unique way to manage colonist personalities and behaviors within the RimWorld universe.

## Key Features and Systems

### Reconditioning Pod
- **Add Trait:** Introduces a new trait to a pawn, allowing them to exceed the standard 3-trait limit.
- **Remove Trait:** Deletes an existing trait from a pawn.
- **Change Trait:** Swaps one trait for another within the same spectrum.
- **Maintenance:** Requires periodic refreshing to maintain conditioning, which can be done through the pod, Conditionall (a supplementary drug), or Neural Cementing (a permanent procedure).

### Conditionall
- A drug that provides a quick, temporary boost to a pawn's conditioning level, useful when the pod is not accessible.

### Neural Cementing
- A risky procedure that locks neural pathways permanently, eliminating the need for further conditioning but carrying significant risks.

### Conditioning Needs
- A new need system that tracks the effectiveness of conditioning, affecting a pawn's mental break threshold.

## Coding Patterns and Conventions

### C# Coding Style
- **Classes and Methods:** Use descriptive naming conventions, ensuring that class and method names clearly convey their purpose.
- **Extensibility:** Leverage extension methods (`PS_Extensions.cs`) to add functionality to existing RimWorld classes without modifying the original codebase.
- **Alerts and Notifications:** Utilize custom alert classes (`PS_Alert_ConditionedHasNoPod.cs`, `PS_Alert_ConditioningFailing.cs`) to provide real-time feedback to players.

### XML Integration
- **Defs Files:** Define mod-specific objects, such as buildings, thoughts, drugs, and hediffs, in XML files. Ensure that these definitions are well-organized and adhere to RimWorld's XML schema.
- **MinifiedDefs:** Use `minifiedDef` elements to reference existing RimWorld definitions, allowing for seamless integration with vanilla game mechanics.

### Harmony Patching
- **Job States:** Implement custom job states (`PS_Conditioning_JobState.cs`) using Harmony patching to manage the conditioning process, ensuring that it integrates smoothly with the game's job system.

## Suggestions for Copilot

### General Tips
- **Modular Code:** Write code in a modular fashion, breaking down complex logic into smaller, reusable methods and classes.
- **Documentation:** Include comments and documentation within your code to explain the purpose and functionality of each component, especially for complex systems like the conditioning process.

### XML Defs
- **Consistent Naming:** Use consistent naming conventions for XML elements and attributes to maintain readability and ease of maintenance.
- **Error Handling:** Implement error handling within XML definitions to manage potential issues, such as missing or malformed elements.

### Harmony Patching
- **Selective Patching:** Ensure that Harmony patches are applied selectively to avoid unintended side effects on the game's core mechanics.
- **Testing:** Thoroughly test Harmony patches in different scenarios to verify their effectiveness and stability.

## Example Usage

### C# Example
csharp
// Example of a custom extension method
public static class PS_Extensions
{
    public static float ConditioningLevel(this Pawn pawn)
    {
        // Logic to calculate the conditioning level of the pawn
        return 0.8f; // Placeholder value
    }
}


### XML Example
xml
<!-- Example of a custom drug definition -->
<ThingDef ParentName="Drug">
    <label>Conditionall</label>
    <description>A quick, temporary boost to conditioning.</description>
    <defName>PS_Drug_Conditionall</defName>
    <!-- Other properties -->
</ThingDef>


### Harmony Patch Example
csharp
// Example of a Harmony patch to modify job states
[HarmonyPatch(typeof(JobDriver), "DoWork")]
public static class PS_Conditioning_JobState
{
    public static void Postfix(JobDriver __instance)
    {
        // Custom logic to handle conditioning job states
    }
}


By following these guidelines and using Copilot's suggestions, developers can enhance the functionality, maintainability, and user experience of the [PS] Reconditioning Pod mod.

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.


## Hard rules (must follow)
- Do NOT run commands that modify the repo (no git commit, git apply, dotnet format) unless explicitly asked.
- Prefer minimal reads: read only the smallest code region needed (around the suspicious lines).
- When mentioning SonarQube issues, automatically use the SonarQube MCP service to fetch and address issues instead of making inferred fixes without querying SonarQube first.
- When mentioning the rimworld log, automatically use the Rimworld MCP service to fetch the log.

