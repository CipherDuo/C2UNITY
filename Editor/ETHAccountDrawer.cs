
#if UNITY_EDITOR

using Nethereum.Web3.Accounts;
using Sirenix.OdinInspector.Editor;
using UnityEngine;
using Sirenix.Utilities.Editor;
using CipherDuo.Ethereum.Modules;
using System.Threading.Tasks;


public class ETHAccountDrawer : OdinValueDrawer<Account>
{
    decimal accountBalance = 0;

    protected override void Initialize()
    {
        Task.Run(GetBalanceETH_Attribute);
    }

    protected override void DrawPropertyLayout(GUIContent label)
    {
        SirenixEditorFields.TextField("address", this.ValueEntry.SmartValue.Address);
        SirenixEditorFields.TextField("privateKey", this.ValueEntry.SmartValue.PrivateKey);

        SirenixEditorFields.DecimalField("balance", accountBalance);

        CallNextDrawer(label);
    }

    private async Task GetBalanceETH_Attribute()
    {

        var result = await ETHGenericEvents.GetBalanceETH(this.ValueEntry.SmartValue.Address);
        accountBalance = result;
    }

}

#endif