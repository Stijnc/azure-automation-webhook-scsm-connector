#
# Refresh-Connector.ps1
#
# Collect Runbooks and DSC Configs from Azure Automation
$TypePath = (Get-ItemProperty "hklm:\software\microsoft\system center\2010\service manager\setup").InstallDirectory + "scsm.azureautomation.wpf.dll"
Add-Type -Path $TypePath

#need to get encrypted password once that is complete
# neeed to update code to allow multipule connectors, this code will only work with one connector currently

$SMClass = Get-SCSMClass -Name SCSM.AzureAutomation.Connector$
$SMObject = Get-SCSMObject -Class $SMClass 
$SubscriptionID = $SMObject.SubscriptionID
$AutomationAccountName = $SMObject.AutomationAccount
$username = $SMObject.RunAsAccountName
$encryptedPassword = $SMObject.RunAsAccountPassword
$secpassword = ConvertTo-SecureString([SCSM.AzureAutomation.WPF.Connector.StringCipher]::Decrypt($encryptedPassword,$username)) -AsPlainText -Force
$ResourceGroup = $SMObject.ResourceGroup

$Creds = New-Object System.Management.Automation.PSCredential ($username, $secpassword)
Login-AzureRmAccount -Credential $Creds -SubscriptionId $SubscriptionID

#Get Each Runbook and Save to CMDB
$Runbooks = Get-AzureRmAutomationRunbook -ResourceGroupName $ResourceGroup -AutomationAccountName $AutomationAccountName 
Foreach($runbook in $Runbooks)
{
	$Runbookobj = Get-AzureRmAutomationRunbook -AutomationAccountName $AutomationAccountName -ResourceGroup $ResourceGroup -Name $runbook.Name
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