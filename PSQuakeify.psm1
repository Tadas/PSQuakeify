function Install-QuakeHotkey {
	[CmdletBinding()]
	Param(
		[int]$WindowHandle = (Get-Process -Id $PID).MainWindowHandle
	)
	$ErrorActionPreference = "Stop"

	Write-Verbose "Reading hotkey handler code"
	$HotkeyHandlerCS = Get-Content (Join-Path $PSScriptRoot "HotkeyHandler.cs") -Raw 

	Write-Verbose "Starting hotkey handler job"
	try {
		$JobInfo = Start-Job -Name "Quakeify" -ArgumentList @([int]$WindowHandle, $HotkeyHandlerCS) -ScriptBlock {
			Param(
				[int]$TargetWindowHandle,
				[string]$HotkeyHandlerWindowCode
			)
			$ErrorActionPreference = "Stop"

			if($TargetWindowHandle -eq $null) { throw "No target window handle" }

			Add-Type -TypeDefinition $HotkeyHandlerWindowCode -ReferencedAssemblies System.Windows.Forms

			$form = New-Object WindowsApplication1.Form1
			$form.TargetConsoleWindow = $TargetWindowHandle
			
			if ($form.RegisterHotkey()){
				Write-Output "Registered" # Let the main script know we're running
				$form.Show();

				while ($true) {
					Start-Sleep -Milliseconds 50
					[System.Windows.Forms.Application]::DoEvents()
				}
			} else {
				Write-Output "Failed"
			}
		}

		# Wait for output from the job
		Write-Verbose "Waiting for job output"
		for ($i = 0; $i -lt 10; $i++) {
			if($JobInfo.HasMoreData){
				$JobOutput = Receive-Job -Id $JobInfo.Id
				Write-Verbose "Received job output: $JobOutput"

				# For some reason you can receive empty results - ignore those
				if([string]::IsNullOrEmpty($JobOutput)){
					Write-Verbose "Received empty output - still waiting"
				} else {
					break
				}
			}
			Write-Verbose "Still waiting for job to return something..."
			Start-Sleep -Milliseconds 200
		}

		if ($JobOutput -ne "Registered"){
			throw "Failed to register hotkey"
		}
		Write-Verbose "Hotkey installed!"

	} catch {
		Remove-Job -Id $JobInfo.Id -Force
		Write-Error "Failed to register hotkey"
	}
}

function Remove-QuakeHotkey {
	Get-Job -Name "Quakeify" | Remove-Job -Force
}