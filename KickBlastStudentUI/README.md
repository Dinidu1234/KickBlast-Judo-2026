# KickBlastStudentUI

## Open in Visual Studio
1. Open **Visual Studio 2022**.
2. Click **Open a project or solution**.
3. Select `KickBlastStudentUI.sln`.

## Run the app
1. Make sure `KickBlastStudentUI` is the startup project.
2. Press **F5** or click **Start**.
3. Login with default account:
   - Username: `rashiii`
   - Password: `123456`

## Change pricing
1. Go to **Settings** page.
2. Update the pricing values.
3. Click **Save Settings**.
4. This updates `appsettings.json` and reloads pricing in memory.

## Reset the database
1. Close the app.
2. Delete this file if it exists: `Data/kickblast_student.db` under the app output folder.
3. Run the app again to recreate tables and seed default data.
