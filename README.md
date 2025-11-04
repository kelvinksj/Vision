# TODO
1. none
<br><br>

# v1.6.11
1. Add filter out "maintenance" from the selection list in area selection in ViewNode.
2. Check in page to include status for load out and check out items.
3. Check-out map page click on box have check out function.
<br><br>

# v1.6.10
1. Add checking for layout name is still being use in layout delete function.
2. Add checking for kit number is still being use in kit delete function.
<br><br>

# v1.6.9
1. Add check out http command to check out button in check out page.
2. Check in page add manual button for location to simulate keyboard.
<br><br>

# v1.6.8
1. Add messages from Task Manager with area ID 9999 for both robot maintenance and table maintenance.
2. Fix robot maintenance page keep refreshing affecting the pagination.
3. Fix scan one and scan all when exit page and go back into the page will still show the scan message.
<br><br>

# v1.6.7
1. Fix check out page to show status "Quarantine" if container condition is "Quantantine".
2. Fix table maintenance page when update contianer the type is "Gaylord" instead of "GL".
3. Fix table maintenance page when click update button will show popup for "Engineer".
4. Fix line run page to show green background for table status for gaylord container type is "Full" type.
5. Fix table maintenance for move http command.
6. Fix amr maintenence for move http command.
7. Fix the line 1 to line 6 position number. Line 1 = 2206, Line 6 =  2201.
<br><br>

# v1.6.6
1. Add http UI command for Location setting to Container Manager.
2. Fix kit layout duplicate to set the status to "".
3. Fix query command for check out and table maintenance for empty list to display as empty table.
4. Add 2 operator mode "check in" and "check out".
5. Change user page to display page column as full text instead of text code. E.g. CPC to display "Checkpoint Charlie".
6. Add table update UI in table maintenance.
7. Change the default IP for RCS to "70.70.70.2" in settings.
<br><br>

# v1.6.5
1. Fix http command to all managers to have .DeepClone so got no ref to variable. Task Manager will get the correct data and not duplicates.
2. Fix cannot clear Gaylord table because showing red box.
3. Add operator user level should not be able to change the line settings.
4. Fix manual check in can add location with no table.
5. Change browser title to "Task Manager UI".
6. Fix completed kit layout should not be able to edit.
<br><br>

# v1.6.4
1. Fix message box for Task Manager messages to handle multi area.
2. Add ui message error check with console message for Task Manager.
3. Add container message error check with comsole message for Container Manager.
4. Fix line command status. Will update the status to hide after the command has finished.
5. Change line run location got table will have background color green and white text
6. Fix setup line when assign not to show those kit layout that have status.
7. Add gaylord as a drop down option in the maintenance check-in.
8. Change gaylord page to follow specific position and have conveyor in the center.
9. Change line run box with position to indicate got table with green background and white text.
10. Change new layout for position 1002 and 1003 to have default FG only.
11. Fix check-in manual key-in location select only need position number.
<br><br>

# v1.6.3
1. CPC add line assignment.
2. CPC add new positions.
3. Add quick setup for layout and kit layout.
4. Add check-in manual key-in.
5. Gaylord add 6 more positions.
<br><br>

# v1.6.2
1. Replenish and clear command for Checkin Page.
2. Fix MsgList box.
3. Login eye icon adjustment.
4. Add operator menu navigation for finished good and gaylord.
5. Scan one page http command change from "one" to "multi"
6. Add Checkin Table at Maintenance page to check in new table.
7. Add change RCS map id in setting.
8. Change http command for maintenance check-in and check-in page request + clear table
<br><br>

# v1.6.1
1. Line run box status and command status.
2. Msgbox icon adjustment.
3. Operator user cannot edit to change the page access.
4. Solved CommonLib.cs MonitorSubscribe high load on the CPU.
<br><br>

# v1.6
1. Checkin inplement multi user ProGlove.
2. Container service add behaviour CheckOut (Checkout Page. and Maintainance (Table Maintainance Page. for query
<br><br>

# v1.5.2
1. Container manager new http for CPC location settings.
<br><br>