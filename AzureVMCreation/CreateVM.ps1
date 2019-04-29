# Shows how to create a VM using powershell script
Connect-AzureRmAccount

if (@($subscriptions).Count > 1) {
    $title="Choose subscription"
    $info="Choose the subscription to use for this demo.  Resources will be created on your behalf"
    $options=[System.Management.Automation.Host.ChoiceDescription[]]$subscriptions
    $optionChosen=$host.UI.PromptForChoice($title, $info, $options, 0)
}
else {
  $optionChosen = $subscriptions
}

$VMLocalAdminUser = "LocalAdminUser"
$VMLocalAdminSecurePassword = ConvertTo-SecureString "LocalAdmin123$" -AsPlainText -Force
$LocationName = "westus"
$ResourceGroupName = "aztest"
$ComputerName = "Bubba"
$VMName = "Bubba"

#  Get a list of VM Sizes in the location using
#  get-azurermvmsize -Location {location}
$VMSize = "Standard_A0"

$NetworkName = "bubbanet"
$NICName = "bubbanic"
$SubnetName = "bubbasubnet"
$SubnetAddressPrefix = "10.0.0.0/24"
$VnetAddressPrefix = "10.0.0.0/16"

New-AzureRmResourceGroup -Name $ResourceGroupName -Location 'North Central US'

$SingleSubnet = New-AzureRmVirtualNetworkSubnetConfig -Name $SubnetName -AddressPrefix $SubnetAddressPrefix
$Vnet = New-AzureRmVirtualNetwork -Name $NetworkName -ResourceGroupName $ResourceGroupName -Location $LocationName -AddressPrefix $VnetAddressPrefix -Subnet $SingleSubnet
$NIC = New-AzureRmNetworkInterface -Name $NICName -ResourceGroupName $ResourceGroupName -Location $LocationName -SubnetId $Vnet.Subnets[0].Id

$Credential = New-Object System.Management.Automation.PSCredential ($VMLocalAdminUser, $VMLocalAdminSecurePassword);

$VirtualMachine = New-AzureRmVMConfig -VMName $VMName -VMSize $VMSize
$VirtualMachine = Set-AzureRmVMOperatingSystem -VM $VirtualMachine -Windows -ComputerName $ComputerName -Credential $Credential -ProvisionVMAgent -EnableAutoUpdate
$VirtualMachine = Add-AzureRmVMNetworkInterface -VM $VirtualMachine -Id $NIC.Id
$VirtualMachine = Set-AzureRmVMSourceImage -VM $VirtualMachine -PublisherName 'MicrosoftWindowsServer' -Offer 'WindowsServer' -Skus '2012-R2-Datacenter' -Version latest

New-AzureRmVM -ResourceGroupName $ResourceGroupName -Location $LocationName -VM $VirtualMachine -Verbose