#
# Get_JobStatus.ps1
#
# Get Job Status for Running Jobs

# This logic could be improved so the refresh of the Job status resumes after a restart of the workflow Service
Import-Module Azure
Import-Module SMLets

$JobPollingTimeoutInSeconds = 3600

Import-Module Azure
Switch-AzureMode AzureResourceManager

# "Add Code here to get password"
# "add Code to get Subscription ID"
#$Params =  "Add Code to Build hash table to pass parameters

$secpassword = ConvertTo-SecureString $password -AsPlainText -force
$Creds = New-Object System.Management.Automation.PSCredential ($username, $secpassword)
$Account = Add-AzureAccount -Credential $Creds
$Subscription = Select-AzureSubscription -SubscriptionId $SubscriptionID
$job = Get-AzureAutomationJob -ResourceGroupName $ResourceGroup -AutomationAccountName $AutomationAccountName -Id $JobID



if ($job -eq $null) { 
        # No job was created, so throw an exception 
        # throw ("No job was created for runbook: $ChildRunbookName.") 
		# Need to Add Code here
} 
else 
{ 
        
# Monitor the job until finish or timeout limit has been reached 
$maxDateTimeout = (Get-Date).AddSeconds($JobPollingTimeoutInSeconds)
             
$doLoop = $true 
             
while($doLoop) { 
    Start-Sleep -s $JobPollingIntervalInSeconds 
                 
    $job = Get-AzureAutomationJob ` 
        -Id $job.Id ` 
        -AutomationAccountName $AutomationAccountName
		-ResourceGroupName $ResourceGroup 
                 
    if ($maxDateTimeout -lt (Get-Date)) { 
        # timeout limit reached so exception 
                
    } 
                 
    $doLoop = (($job.Status -notmatch "Completed") -and ($job.Status -notmatch "Failed") -and ($job.Status -notmatch "Suspended") -and ($job.Status -notmatch "Stopped")) 
} 
             
    if ($job.Status -match "Completed") { 
        if ($ReturnJobOutput) { 
            # Output 
            $jobout = Get-AzureAutomationJobOutput ` 
                            -Id $job.Id ` 
                            -AutomationAccountName $AutomationAccountName `
							-ResourceGroupName $ResourceGroup ` 
                            -Stream Output 
            if ($jobout) {Write-Output $jobout.Text} 
                     
            # Error 
            $jobout = Get-AzureAutomationJobOutput ` 
                            -Id $job.Id ` 
                            -AutomationAccountName $AutomationAccountName `
							-ResourceGroupName $ResourceGroup ` 
                            -Stream Error 
            if ($jobout) {Write-Error $jobout.Text} 
                     
            # Warning 
            $jobout = Get-AzureAutomationJobOutput ` 
                            -Id $job.Id ` 
                            -AutomationAccountName $AutomationAccountName ` 
							-ResourceGroupName $ResourceGroup `
                            -Stream Warning 
            if ($jobout) {Write-Warning $jobout.Text} 
                     
            # Verbose 
            $jobout = Get-AzureAutomationJobOutput ` 
                            -Id $job.Id ` 
                            -AutomationAccountName $AutomationAccountName `
							-ResourceGroupName $ResourceGroup ` 
                            -Stream Verbose 
            if ($jobout) {Write-Verbose $jobout.Text} 
        } 
        else { 
            # Return the job id 
                    
        } 
    } 
    else { 
        # The job did not complete successfully, so throw an exception 
               
    } 
}

# Add Code to Update the SCSM Runbook Activity        






