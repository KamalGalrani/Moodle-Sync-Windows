schtasks /create /sc hourly /mo 3 /sd 20/01/2014 /tn "Moodle Sync" /tr "wscript \"%USERPROFILE%\AppData\Moodle Sync\Callback.vbs\"" /F
