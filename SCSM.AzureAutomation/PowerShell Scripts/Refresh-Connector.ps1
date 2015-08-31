#
# Refresh-Connector.ps1
#
# Collect Runbooks and DSC Configs from Azure Automation
Import-Module SMLets
Import-Module Azure
Switch-AzureMode AzureResourceManager

# "Add Code here to get password"
# "add Code to get Subscription ID"
# Get Connector ID
#$Params =  "Add Code to Build hash table to pass parameters

$secpassword = ConvertTo-SecureString $password -AsPlainText -force
$Creds = New-Object System.Management.Automation.PSCredential ($username, $secpassword)
$Account = Add-AzureAccount -Credential $Creds
$Subscription = Select-AzureSubscription -SubscriptionId $SubscriptionID

#Get Each Runbook and Save to CMDB
$Runbooks = Get-AzureAutomationRunbook -ResourceGroupName $ResourceGroup -AutomationAccountName $AutomationAccountName 
Foreach($runbook in $Runbooks)
{
	$Runbookobj = Get-AzureAutomationRunbook -AutomationAccountName $AutomationAccountName -ResourceGroup $ResourceGroup -Name $runbook.Name
	$RunbookHT = @{
		ConnectorID = $ConnectorID
		Name = $Runbookobj.Name
		Description = $Runbookobj.Description
		LastModifiedtime = $Runbookobj.LastModifiedTime
		LogVerbose =$Runbookobj.LogVerbose
		LogProgress = $Runbookobj.LogProgress
		CreatedDate = $Runbookobj.CreationTime
		RunbookType = $Runbookobj.RunbookType
		JobCount = $Runbookobj.JobCount
	}
	$SMRBClass = Get-SCSMClass -Name SCSM.AzureAutomation.Runbook$ 
	New-SCSMObject -Class $SMRBClass -PropertyHashtable $RunbookHT

}

#Get Each DSC Configuration and Save to CMDB
 ## Todo later