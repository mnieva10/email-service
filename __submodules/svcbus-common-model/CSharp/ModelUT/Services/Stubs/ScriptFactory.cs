using System.Collections.Generic;

namespace ModelUT.Services.Stubs
{
    public class ScriptFactory
    {
        public readonly List<string> DefaultParams = new List<string>{
            @"Enabled=False,Customizable=False,Visible=False,ReadOnly=True,""VARIABLE_$REJECT NAME=Farhan"",""VARIABLE_SOURCE FILE=C:\Recived Files\Farhana\BASSOT.txt"",""VARIABLE_TEXT FILE LAYOUT=BASS03.FX1"",""VARIABLE_$TEXT FILE LAYOUT=BASS03.FX1"",VARIABLE_MAP=BASS-far.MP1,VARIABLE_$MAP=BASS-far.MP1,""VARIABLE_$SOURCE TYPE=Fixed length text"",""VARIABLE_SOURCE TYPE=Fixed length text"",VARIABLE_$$FCHAR=1,DEFINE_$MATCH.SUM=N,""DEFINE_$CHECK ZIP CODE=N"",DEFINE_$MATCH.RECNO=N,DEFINE_$MATCH.TIN=Y,DEFINE_$MATCH.GML=N",
            @"Enabled=False,Customizable=False,Visible=False,ReadOnly=True,VARIABLE_MAP=Gendata.MPM,VARIABLE_$MAP=Gendata.MPM,""VARIABLE_TEXT FILE LAYOUT=GENDATA.FXM"",""VARIABLE_$TEXT FILE LAYOUT=GENDATA.FXM"",""VARIABLE_SOURCE FILE=X:\CS\Run\Data\Data1k_t.txt"",""VARIABLE_$SOURCE TYPE=Fixed length text"",""VARIABLE_SOURCE TYPE=Fixed length text"",VARIABLE_$$FCHAR=M,""VARIABLE_$REJECT NAME="",""VARIABLE_$ReRunResult=_IMPORTS.TAX FORM.VIEWS.LAST IMPORT"",""VARIABLE_$ImportResult=_IMPORTS.TAX FORM.VIEWS.LAST IMPORT"",""VARIABLE_$TestResult=_IMPORTS.TAX FORM.VIEWS.TEST IMPORT"",VARIABLE_$Password=,""DEFINE_$OVERRIDE ASSIGN ALTSTATE=Y"",DEFINE_$MATCH.SUM=Y,""DEFINE_$PTIN IN LOOKUP=Y"",""DEFINE_$GENERATE AUDIT=Y"",DEFINE_$REPRINTIND=N,DEFINE_$MATCH.RECNO=N,DEFINE_$MATCH.TIN=Y,DEFINE_$Match.PTIN=Y"
        }; 

        public readonly List<string> ParametersData = new List<string>()
            {
                @"$SelectFields=ACCOUNTNO,SOURCE,STATE,DATEMODIFIED,TRANDATE,RECNO,MODLEVEL,PROCEEDS,BARTER,FEDWITHHELD,RPLTXYR,COSTBASIS,WASHSALELOSS,UPLLAST1231,UPLCUR1231,TTLPL,STWITHHELD,LOCWITHHELD,AUXAMT1,AUXAMT2,AUXAMT3,PMAYRPL,PMAY5TTLPL,INFO1,INFO2
                $$FCHAR=B
                $$Tbl=AUDIT
                $$AUDIT=Y
                ",

                @"$SelectFields=CHANGEDATE,USERID,TIN,TINTYPE,ACCOUNTNO,GROUPNO,NAME,NAME2,ADDRESS,ADDRESS2,CITY,STATE,ZIP,ZIPEXT,RESCOUNTRYCODE,PROVINCECODE,INFO1,INFO2,CODE1,CODE2,EIN,MODLEVEL
                $$Tbl=AUDIT
                $$AUDIT=T
                $$DRILLDOWNDOCKED=N
                $$DRILLDOWNENABLED=N
                $$DRILLDOWNFORM=
                $DEFAULTWIZZARD=Customize
                $hctDefault=260
                ", 

                //paramstr
                @"State=PR
                DocumentType=
                Transmitter=
                $Quarter=
                $Optional Code=
                $LocalityName=
                ForceAgentWith=
                PIN=N
                FinalReturn=N
                $MagTapeFile=N
                $PriorYear=N
                $TestFile=N
                $CombFedSt=N
                $Locality=N
                $FederalLimits=N
                $3rdPartySickPay=N"
            };

        public readonly List<string> PreParsedScripts = new List<string>()
            {
                @"$$$Security.CustomGroups=
                $$$Security.All_PReadOnlyList=P2,TRANSMITTED
                $$$Security.All_PShowList=
                $$$Security.SamePrtyProperty=1
                $$$Security.WebPriority=5",

                @"$$$Security.CustomGroups=
                $$$Security.All_GReadOnlyList=
                $$$Security.All_GShowList=
                $$$Security.SamePrtyProperty=2
                $$$Security.WebPriority=5", 

                @"$$$Security.CustomGroups=AdminGroup1
                $$$DisableForms= 
                $$$Security.All_PReadOnlyList=9001
                $$$Security.All_PShowList=
                $$$Security.Payer_PReadOnlyList=
                $$$Security.Payer_PShowList=
                $$$Security.WebPriority=7",

                @"$$$Security.CustomGroups=
                $$$Security.All_GReadOnlyList=
                $$$Security.All_GShowList=",

                @"$$$Security.CustomGroups=
                $$$Security.All_GReadOnlyList=
                $$$Security.All_GShowList=
                $$$Security.WebPriority=",
            };
    }
}
