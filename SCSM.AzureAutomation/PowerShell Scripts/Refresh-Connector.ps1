#
# Refresh-Connector.ps1
#
# Collect Runbooks and DSC Configs from Azure Automation
Import-Module SMLets
Import-Module Azure
Switch-AzureMode AzureResourceManager

#need to get encrypted password once that is complete
# neeed to update code to allow multipule connectors, this code will only work with one connector currently

$SMClass = Get-SCSMClass -Name SCSM.AzureAutomation.Connector$
$SMObject = Get-SCSMObject -Class $SMClass 
$SubscriptionID = $SMObject.SubscriptionID
$AutomationAccountName = $SMObject.AutomationAccount
$password = $SMObject.RunAsAccountPassword
$username = $SMObject.RunAsAccountName
$ResourceGroup = $SMObject.ResourceGroup

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
	$Runbook = Get-SCSMObject -Class $SMRBClass -Filter 'Runbook -eq $Runbookobj.Name'
	if($Runbook -eq $null)
	{
		New-SCSMObject -Class $SMRBClass -PropertyHashtable $RunbookHT
	}
	

}

#Get Each DSC Configuration and Save to CMDB
 ## Todo later