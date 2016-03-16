#
# Refresh-Connector.ps1
#
# Collect Runbooks and DSC Configs from Azure Automation
$TypePath = (Get-ItemProperty "hklm:\software\microsoft\system center\2010\service manager\setup").InstallDirectory + "scsm.azureautomation.wpf.dll"
Add-Type -Path $TypePath

#need to get encrypted password once that is complete
# neeed to update code to allow multipule connectors, this code will only work with one connector currently
$SMRBClass = Get-SCSMClass -Name SCSM.AzureAutomation.Runbook$
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
	if ($Runbookobj.State -eq "New")
	{
		$RBType = switch($Runbookobj.RunbookType)
		{
			Script{"AzureAutomationRunbook.Type.Workflow"}
			Graph{"AzureAutomationRunbook.Type.Graphical"}
			PowerShell{"AzureAutomationRunbook.Type.PowerShell"}
		}

		$param = @()
        ($Runbookobj.Parameters).GetEnumerator() | Foreach-object { 
            $object = [PSObject]@{
                Parameter = $_.Name
                DefaultValue = ($_.Value).DefaultValue
                IsMandatory = ($_.Value).IsMandatory
                Position = ($_.Value).Position  
                Type = ($_.Value).Type 
                }
            $param += $object
        }
        $JsonParam = $param | ConvertTo-Json

		$RunbookHT = @{
			#ConnectorID = $ConnectorID
            DisplayName = $Runbookobj.Name
			Name = $Runbookobj.Name
			Description = $Runbookobj.Description
			LastModifiedtime = $Runbookobj.LastModifiedTime
			LogVerbose =$Runbookobj.LogVerbose
			LogProgress = $Runbookobj.LogProgress
			CreatedDate = $Runbookobj.CreationTime
			RunbookType = $RBType
			#JobCount = $Runbookobj.JobCount
			Status = "AzureAutomationRunbook.Status.Published"
			Parameters = $JsonParam.ToString()
		}

		$rbName = $Runbookobj.Name
		$Runbook = Get-SCSMObject -Class $SMRBClass -Filter "Name -eq $rbName"
		if($Runbook -eq $null)
		{
			New-SCSMObject -Class $SMRBClass -PropertyHashtable $RunbookHT
		}
        else {
            Set-SCSMObject -SMObject $Runbook -PropertyHashtable $RunbookHT
        }
	}
	
	
	

}

#Get Each DSC Configuration and Save to CMDB
 ## Todo later