# FixBuildAndRun

If you get run/build issues, follow these steps:

1. **Set Startup Project**
   - Right-click `KickBlastStudentUI` project > **Set as Startup Project**.

2. **Clean/Rebuild**
   - Build menu > **Clean Solution**.
   - Build menu > **Rebuild Solution**.

3. **Delete bin/obj manually**
   - Close Visual Studio.
   - Delete `bin` and `obj` folders inside `KickBlastStudentUI`.
   - Reopen solution and build.

4. **Verify launchSettings**
   - Ensure `Properties/launchSettings.json` contains one profile with `"commandName": "Project"` only.
   - No `executablePath`.

5. **Debug target**
   - Use **Any CPU** in toolbar configuration.
