using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System;
using System.IO;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using static UnityEngine.GUILayout;
using UnityEngine.Rendering;
using Steamworks;
using rail;
using System.Runtime.Remoting.Contexts;
using TranslationCommon.SimpleJSON;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace DSPMoreRecipes
{

    [BepInPlugin("Appun.DSP.plugin.MoreRecipes", "DSPMoreRecipes", "1.0.3")]
    [BepInProcess("DSPGAME.exe")]



    public class DSPMoreRecipes : BaseUnityPlugin
    {
        public static int selectedRecipeID = -1;
        public static int GridIndex = 0;

        public static Dictionary<int, int> GridIndexDictionary = new Dictionary<int, int>();
        public static Dictionary<int, int> GridIndexDictionaryOrigin = new Dictionary<int, int>();


        public static string PluginPath = System.IO.Path.GetDirectoryName(
                   System.Reflection.Assembly.GetExecutingAssembly().Location);
        public static int X;
        public static int Y;
        public static bool resized1 = false;
        public static bool resized2 = false;
        public static bool resized3 = false;
        public static bool resized4 = false;

        public static RectTransform rectTrans;
        public static Sprite newIcon;
        public static UIButton typeButton3;
        public static Text text;


        //public static Image handIconImage;

        public static GameObject EditModeTextObj;


        public static GameObject destRecipeSelImageObj;

        //public static GameObject moveButtonObj;
        public static GameObject resetButtonObj;
        public static GameObject editButtonObj;
        public static GameObject selectedRecipeIcon;


        //public static Button moveButton;
        public static Button resetButton;
        public static Button editButton;
        public static Text moveText;
        public static Text resetText;
        public static Text editText;
        public static bool editMode = false;

        public static int selGridInt = 0;
        Vector2 scrollPosition = Vector2.zero;
        public static int ItemLaw;

        //public static RecipeProto[] RecipeDataBakUp;

        public static string jsonFilePath;

        //public static int colCount = 20;

        //スタート
        public void Start()
        {
            LogManager.Logger = Logger;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());


            jsonFilePath = Path.Combine(PluginPath, "GridIndex.json");
            //LogManager.Logger.LogInfo("target path " + jsonFilePath);
            if (!File.Exists(jsonFilePath))
            {
                //LogManager.Logger.LogInfo("File not found" + jsonFilePath);
                File.WriteAllText(jsonFilePath, JSON.ToJson("{}"));
            }
            else
            {
                GridIndexDictionary = JSON.FromJson<Dictionary<int, int>>(File.ReadAllText(jsonFilePath));
                //LogManager.Logger.LogInfo("GridIndex dictionary load finish.");
            }


            //ボタンを追加
            GameObject logicbutton = GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Station Window/storage-box-0/popup-box/sd-option-button-0");
            GameObject recipegroup = GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Replicator Window/recipe-group");
            resetButtonObj = Instantiate(logicbutton) as GameObject;
            editButtonObj = Instantiate(logicbutton) as GameObject;
            resetButtonObj.SetActive(true);
            editButtonObj.SetActive(true);

            resetButtonObj.name = "resetButtonObj";
            editButtonObj.name = "editButtonObj";

            resetButtonObj.transform.SetParent(recipegroup.transform, false);
            editButtonObj.transform.SetParent(recipegroup.transform, false);

            resetButtonObj.transform.localPosition = new Vector3(315, 65, 0);
            editButtonObj.transform.localPosition = new Vector3(295,  25, 0);

            RectTransform resetButtonRT = resetButtonObj.GetComponent<RectTransform>();
            RectTransform editButtonRT = editButtonObj.GetComponent<RectTransform>();

            resetButtonRT.sizeDelta = new Vector2(80, 25);
            editButtonRT.sizeDelta = new Vector2(100, 25);

            resetButton = resetButtonObj.GetComponent<Button>();
            editButton = editButtonObj.GetComponent<Button>();

            Image resetButtonImage = resetButtonObj.GetComponent<Image>();
            Image editButtonImage = editButtonObj.GetComponent<Image>();

            resetButtonImage.color = new Color(1.0f, 0.68f, 0.45f, 0.7f);
            editButtonImage.color = new Color(0.240f, 0.55f, 0.65f, 0.7f);



            resetText = resetButtonObj.GetComponentInChildren<Text>();
            editText = editButtonObj.GetComponentInChildren<Text>();
                            resetText.text = "Reset All".Translate();
                editText.text = "Enter Edit Mode".Translate();


            resetText.name = "resetText";
            editText.name = "editText";

            resetButton.onClick.AddListener(OnClickResetButton);
            editButton.onClick.AddListener(OnClickEditButton);


            //選択アイコン用BG　
            GameObject selimg = GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Replicator Window/recipe-group/sel-img");
            destRecipeSelImageObj = Instantiate(selimg) as GameObject;
            destRecipeSelImageObj.transform.SetParent(recipegroup.transform, false);
            destRecipeSelImageObj.SetActive(false);

            //アイコン用画像
            selectedRecipeIcon = Instantiate(GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Replicator Window/recipe-tree/center-icon")) as GameObject;
            selectedRecipeIcon.transform.SetParent(recipegroup.transform, false);
            selectedRecipeIcon.transform.localPosition = new Vector3(200, 36, 0);
            selectedRecipeIcon.transform.Find("place-text").GetComponentInChildren<Text>().text = "Selected Recipe".Translate();
            Destroy(selectedRecipeIcon.transform.Find("vline-m").gameObject);
            Destroy(selectedRecipeIcon.transform.Find("hline-0").gameObject);
            Destroy(selectedRecipeIcon.transform.Find("hline-1").gameObject);
            Destroy(selectedRecipeIcon.transform.Find("icon 2").gameObject);
            Destroy(selectedRecipeIcon.transform.Find("text 1").gameObject);
            Destroy(selectedRecipeIcon.transform.Find("text 2").gameObject);
            Destroy(selectedRecipeIcon.transform.Find("time-text").gameObject);
            Destroy(selectedRecipeIcon.transform.Find("time-text").gameObject);
            selectedRecipeIcon.SetActive(false);
            //モードボタンのコピー

            GameObject modetext = GameObject.Find("UI Root/Overlay Canvas/In Game/Function Panel/bg-trans/mode-text");
            EditModeTextObj = Instantiate(modetext) as GameObject;
            EditModeTextObj.transform.SetParent(recipegroup.transform, false);
            EditModeTextObj.transform.localPosition = new Vector3(0, -3, 0);
            Destroy(EditModeTextObj.GetComponent<Localizer>());
            EditModeTextObj.GetComponent<Text>().text = "Edit mode".Translate();




        }

        //resetボタンクリック
        public void OnClickResetButton()
        {

            //LogManager.Logger.LogInfo("reset");
            foreach (var recipe in LDB.recipes.dataArray)
            {
                recipe.GridIndex = GridIndexDictionaryOrigin[recipe.ID];
            }
            GridIndexDictionary.Clear();
            File.WriteAllText(jsonFilePath, JSON.ToJson("{}"));

            UIRoot.instance.uiGame.replicator.OnTechUnlocked(0, 0,true);
            editMode = true;
        }

        //editボタンクリック

        public void OnClickEditButton()
        {
            if (!editMode)
            {
                EditModeTextObj.SetActive(true);
                //LogManager.Logger.LogInfo("Edit mode on");
                resetButtonObj.SetActive(true);
                editText.text = "Exit Edit Mode".Translate();
                editMode = true;
                destRecipeSelImageObj.gameObject.SetActive(true);
                ref Image recipeSelImage = ref AccessTools.FieldRefAccess<UIReplicatorWindow, Image>(UIRoot.instance.uiGame.replicator, "recipeSelImage");
                recipeSelImage.gameObject.SetActive(false);
                selectedRecipeIcon.SetActive(true);
                selectedRecipeIcon.transform.Find("icon 1").gameObject.SetActive(false);
                destRecipeSelImageObj.gameObject.SetActive(false);
                ref RecipeProto selectedRecipe = ref AccessTools.FieldRefAccess<UIReplicatorWindow, RecipeProto>(UIRoot.instance.uiGame.replicator, "selectedRecipe");
                selectedRecipe = null;

                UIRoot.instance.uiGame.replicator.treeTweener1.Play1To0Continuing();
                UIRoot.instance.uiGame.replicator.treeTweener2.Play1To0Continuing();
            }
            else
            {
                EditModeTextObj.SetActive(false);
                //LogManager.Logger.LogInfo("Edit mode off");
                resetButtonObj.SetActive(false);
                editText.text = "Enter Edit Mode".Translate();
                editMode = false;
                selectedRecipeID = -1;
                destRecipeSelImageObj.gameObject.SetActive(false);
                selectedRecipeIcon.SetActive(false);
            }
        }


        [HarmonyPatch(typeof(UIReplicatorWindow), "_OnInit")]
        public static class UIReplicatorWindow_OnInit_PrePatch
        {
            [HarmonyPrefix]
            public static void Pretfix()
            {

                ref Text[] queueNumTexts = ref AccessTools.FieldRefAccess<UIReplicatorWindow, Text[]>(UIRoot.instance.uiGame.replicator, "queueNumTexts");
                Array.Resize(ref queueNumTexts, 17);

                LogManager.Logger.LogInfo("queueNumTexts.Length = " + queueNumTexts.Length);

            }

        }

        //[HarmonyPatch(typeof(UIReplicatorWindow), "_OnCreate")]
        public static class UIReplicatorWindow_OnCreate_PostPatch
        {
            [HarmonyPostfix]
            public static void Postfix()
            {

                ref Text[] queueNumTexts = ref AccessTools.FieldRefAccess<UIReplicatorWindow, Text[]>(UIRoot.instance.uiGame.replicator, "queueNumTexts");
                Array.Resize(ref queueNumTexts, 17);

                LogManager.Logger.LogInfo("queueNumTexts.Length = " + queueNumTexts.Length);

            }

        }



        //クリック前の処理：２回目にどこでもクリックできるように 問題なし
        [HarmonyPatch(typeof(UIReplicatorWindow), "TestMouseRecipeIndex")]
        public static class UIReplicatorWindow_TestMouseRecipeIndex_retPatch

        {
            [HarmonyPrefix]
            public static bool Prefix()
            {

                if (selectedRecipeID != -1 && editMode == true)
                {
                    

                    //ref uint[] recipeStateArray = ref AccessTools.FieldRefAccess<UIReplicatorWindow, uint[]>(UIRoot.instance.uiGame.replicator, "recipeStateArray");
                    ref int mouseRecipeIndex = ref AccessTools.FieldRefAccess<UIReplicatorWindow, int>(UIRoot.instance.uiGame.replicator, "mouseRecipeIndex");
                    ref Image recipeBg = ref AccessTools.FieldRefAccess<UIReplicatorWindow, Image>(UIRoot.instance.uiGame.replicator, "recipeBg");
                    Vector2 vector;
                    UIRoot.ScreenPointIntoRect(Input.mousePosition, recipeBg.rectTransform, out vector);
                    int num = Mathf.FloorToInt(vector.x / 46f);
                    int num2 = Mathf.FloorToInt(-vector.y / 46f);
                    if (num >= 0 && num2 >= 0 && num < 17 && num2 < 7)
                    {
                        mouseRecipeIndex = num + num2 * 17;
                        //recipeStateArray[mouseRecipeIndex] |= 1U;
                        destRecipeSelImageObj.gameObject.SetActive(true);

                        destRecipeSelImageObj.GetComponent<RectTransform>().anchoredPosition = new Vector2((float)(num * 46 - 1), (float)(-(float)num2 * 46 + 1));


                        //LogManager.Logger.LogInfo("mouseRecipeIndex : " + mouseRecipeIndex);
                        //LogManager.Logger.LogInfo("TestMouseRecipeIndex : " + mouseRecipeIndex);

                    }

                    return false;



                }else
                {
                    return true;

                }

            }
        }





        //クリック後：マウスからGridIndexを取得　どこでもクリックできるように
        [HarmonyPatch(typeof(UIReplicatorWindow), "SetSelectedRecipeIndex")]
        public static class UIReplicatorWindow_SetSelectedRecipeIndex_PrePatch

        {
            [HarmonyPrefix]
            public static bool Prefix(int index)　
            {
                //LogManager.Logger.LogInfo("SetSelectedRecipeIndex Index : " + index);

                GridIndex = 0;
                ref RecipeProto selectedRecipe = ref AccessTools.FieldRefAccess<UIReplicatorWindow, RecipeProto>(UIRoot.instance.uiGame.replicator, "selectedRecipe");
                if (editMode == true )
                {



                    if (selectedRecipeID == -1 ) //１度目のクリック
                    {
                        if ( selectedRecipe != null)
                        {
                            ref Image recipeSelImage = ref AccessTools.FieldRefAccess<UIReplicatorWindow, Image>(UIRoot.instance.uiGame.replicator, "recipeSelImage");
                            recipeSelImage.rectTransform.anchoredPosition = new Vector2((float)(index % 17 * 46 - 1), (float)(-(float)(index / 17) * 46 + 1));
                            recipeSelImage.gameObject.SetActive(true);
                            selectedRecipeID = selectedRecipe.ID;
                            //LogManager.Logger.LogInfo("origin grid selecteed : " + selectedRecipeID);
                            //LogManager.Logger.LogInfo("１度目のクリック" + selectedRecipeID);
                            selectedRecipeIcon.transform.Find("icon 1").gameObject.SetActive(true);
                            selectedRecipeIcon.transform.Find("icon 1").GetComponentInChildren<Image>().sprite = LDB.recipes.Select(selectedRecipeID).iconSprite;
                        } else
                        {

                        return true;
                        }
                    }
                    else　//２度目のクリック　→　移動処理
                    {

                        ref int currentType = ref AccessTools.FieldRefAccess<UIReplicatorWindow, int>(UIRoot.instance.uiGame.replicator, "currentType");
                        //ref uint[] recipeIndexArray = ref AccessTools.FieldRefAccess<UIReplicatorWindow, uint[]>(UIRoot.instance.uiGame.replicator, "recipeIndexArray ");
                        
                        destRecipeSelImageObj.GetComponent<RectTransform>().anchoredPosition = new Vector2((float)(index % 17 * 46 - 1), (float)(-(float)(index / 17) * 46 + 1));
                        //destRecipeSelImageObj.rectTransform.anchoredPosition = new Vector2((float)(index % 17 * 46 - 1), (float)(-(float)(index / 17) * 46 + 1));
                        VFAudio.Create("ui-click-0", null, Vector3.zero, true, 0);
                        int num1 = index / 17 + 1;
                        int num2 = index % 17 + 1;
                        GridIndex = currentType * 1000 + num1 * 100 + num2;
                        //LogManager.Logger.LogInfo("destiantion grid selecteed : " + GridIndex);
                        //originRecipeSelImageObj.gameObject.SetActive(true);

                        //移動処理
                        if (selectedRecipe != null) //2つ目もレシピがあるなら入れ替え
                        {
                            // LogManager.Logger.LogInfo("入れ替え　" + selectedRecipeID + " => " + GridIndex);
                           int tmpGridIndex = LDB.recipes.Select(selectedRecipeID).GridIndex;
                            LDB.recipes.Select(selectedRecipeID).GridIndex = LDB.recipes.Select(selectedRecipe.ID).GridIndex;
                            LDB.recipes.Select(selectedRecipe.ID).GridIndex = tmpGridIndex;
                            GridIndexDictionary[selectedRecipeID] = GridIndex;
                            GridIndexDictionary[selectedRecipe.ID] = tmpGridIndex;
 
                            //moveText.text = "Select Recipe and Click Me".Translate();



                        } else　//２つ目が空欄なら移動

                        {
                            //LogManager.Logger.LogInfo("空欄へ移動　" + selectedRecipeID + " => 空欄:" + GridIndex);
                            LDB.recipes.Select(selectedRecipeID).GridIndex = GridIndex;
                            selectedRecipe = LDB.recipes.Select(selectedRecipeID);
                            GridIndexDictionary[selectedRecipeID] = GridIndex;

                        }
                        destRecipeSelImageObj.gameObject.SetActive(false);
                        ref Image recipeSelImage = ref AccessTools.FieldRefAccess<UIReplicatorWindow, Image>(UIRoot.instance.uiGame.replicator, "recipeSelImage");
                        recipeSelImage.gameObject.SetActive(false);
                        UIRoot.instance.uiGame.replicator.OnTechUnlocked(0, 0, true);
                        File.WriteAllText(jsonFilePath, JSON.ToJson(GridIndexDictionary));
                        selectedRecipeID = -1;



                        selectedRecipeIcon.transform.Find("icon 1").gameObject.SetActive(false);

                    }
                    return false;


                }
                else
                {
                    return true;
                }
            }
        }


        //タイプボタンをクリックしたらレシピ選択を解除
        [HarmonyPatch(typeof(UIReplicatorWindow), "OnTypeButtonClick")]
        public static class UIReplicatorWindow_OnTypeButtonClick_Postfix
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                if (editMode == true)
                {

                    ref RecipeProto selectedRecipe = ref AccessTools.FieldRefAccess<UIReplicatorWindow, RecipeProto>(UIRoot.instance.uiGame.replicator, "selectedRecipe");
                    selectedRecipe = null;
                    selectedRecipeID = -1;
                    ref Image recipeSelImage = ref AccessTools.FieldRefAccess<UIReplicatorWindow, Image>(UIRoot.instance.uiGame.replicator, "recipeSelImage");
                    recipeSelImage.gameObject.SetActive(false);
                    destRecipeSelImageObj.gameObject.SetActive(false);


                }
            }
        }


        //ボタンの文字の修正
        [HarmonyPatch(typeof(UIReplicatorWindow), "_OnOpen")]
        public static class UIReplicatorWindow_OnOpen_PostPatch
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                //LogManager.Logger.LogInfo("UIRecipePicker_OnInit");
                resetButtonObj.SetActive(false);
                editButtonObj.SetActive(true);
            }
        }



        //jsonを元にGridIndexの変更
        [HarmonyPatch(typeof(VFPreload), "InvokeOnLoadWorkEnded")]
        public static class VFPreload_InvokeOnLoadWorkEnded_Patch
        {
            [HarmonyPostfix]
            [HarmonyPriority(1)]

            public static void Postfix()
            {

                //RecipeDataBakUp = LDB.recipes.dataArray;

                foreach (var recipe in LDB.recipes.dataArray)
                {

                    //LogManager.Logger.LogInfo("#################################################   " + recipe.ID );
                    if (recipe == null)
                        continue;
                    GridIndexDictionaryOrigin[recipe.ID] = recipe.GridIndex;

                    if (GridIndexDictionary.ContainsKey(recipe.ID))
                    {
                        //LogManager.Logger.LogInfo("######################################################################   " + recipe.GridIndex + " => " + GridIndexDictionary[recipe.ID]);
                        recipe.GridIndex = GridIndexDictionary[recipe.ID];

                    }

                    if (!resized2)
                    {
                        ref RectTransform windowRect = ref AccessTools.FieldRefAccess<UIReplicatorWindow, RectTransform>(UIRoot.instance.uiGame.replicator, "windowRect");//670 : 811
                        //LogManager.Logger.LogInfo("windowRect  " + windowRect.sizeDelta.x + " : " + windowRect.sizeDelta.y );
                        windowRect.sizeDelta = new Vector2(windowRect.sizeDelta.x + 46 * 5, windowRect.sizeDelta.y);
                        ref RectTransform recipeGroup = ref AccessTools.FieldRefAccess<UIReplicatorWindow, RectTransform>(UIRoot.instance.uiGame.replicator, "recipeGroup"); // 552 : 322
                        //LogManager.Logger.LogInfo("recipeGroup  " + recipeGroup.sizeDelta.x + " : " + recipeGroup.sizeDelta.y);
                        recipeGroup.sizeDelta = new Vector2(recipeGroup.sizeDelta.x + 46 * 5, recipeGroup.sizeDelta.y);
                        ref RectTransform pickerTrans = ref AccessTools.FieldRefAccess<UIRecipePicker, RectTransform>(UIRoot.instance.uiGame.recipePicker, "pickerTrans");
                        //LogManager.Logger.LogInfo("pickerTrans  " + pickerTrans.sizeDelta.x + " : " + pickerTrans.sizeDelta.y);
                        pickerTrans.sizeDelta = new Vector2(pickerTrans.sizeDelta.x + 46 * 5, pickerTrans.sizeDelta.y);
                        ref RectTransform recipeGroup2 = ref AccessTools.FieldRefAccess<UIAssemblerWindow, RectTransform>(UIRoot.instance.uiGame.assemblerWindow, "recipeGroup");
                        //LogManager.Logger.LogInfo("recipeGroup2  " + recipeGroup2.sizeDelta.x + " : " + recipeGroup2.sizeDelta.y);
                        recipeGroup2.sizeDelta = new Vector2(recipeGroup2.sizeDelta.x + 46 * 5, recipeGroup2.sizeDelta.y);

                        //キューグリッド
                        GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Replicator Window/queue-group").GetComponentInChildren<RectTransform>().sizeDelta = new Vector2(552 + 46 * 5, 46);
                        //GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Replicator Window/queue-group/icons-combine").transform.localPosition = new Vector3(1.414f, 1, 1);



                        resized2 = true;
                    }



                }
            }

        }

        //アセンブラーなどのレシピ選択画面のサイズ修正
        [HarmonyPatch(typeof(UIRecipePicker), "_OnOpen")]

        public static class UIRecipePicker_OnOpen_PostPatch
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                //LogManager.Logger.LogInfo("UIRecipePicker_OnInit");
                if (!resized3)
                {
                    foreach (GameObject obj in UnityEngine.Object.FindObjectsOfType(typeof(GameObject)))
                    {
                        if (obj.name.Contains("Recipe"))
                        {
                            foreach (Transform childTransform in obj.transform)
                            {
                                if (childTransform.name.Contains("content"))
                                {
                                    //LogManager.Logger.LogInfo("OBJECT : " + childTransform.name + " x: " + childTransform.GetComponent<RectTransform>().sizeDelta.x + " y: " + childTransform.GetComponent<RectTransform>().sizeDelta.y);

                                    childTransform.GetComponent<RectTransform>().sizeDelta = new Vector2(childTransform.GetComponent<RectTransform>().sizeDelta.x + 46 * 5, childTransform.GetComponent<RectTransform>().sizeDelta.y);

                                    //LogManager.Logger.LogInfo("OBJECT : " + childTransform.name + " x: " + childTransform.GetComponent<RectTransform>().sizeDelta.x + " y: " + childTransform.GetComponent<RectTransform>().sizeDelta.y);
                                    resized3 = true;
                                }
                            }
                        }
                    }
                }

            }

        }

        //[HarmonyPatch(typeof(UIReplicatorWindow), "_OnCreate")]
        class Transpiler_replace
        {
            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);
                if (codes[239].opcode == OpCodes.Ldc_I4_S)
                {
                    LogManager.Logger.LogInfo("codes[239].operand = " + codes[239].operand);
                    codes[239].operand = 17;
                    LogManager.Logger.LogInfo("codes[239].operand = " + codes[239].operand);

                }
                return codes.AsEnumerable();

            }
        }



        //列数の調整一括処理_sbyte
        [HarmonyPatch]
        class Transpiler_replace_12to15_sbyte
        {
            [HarmonyTargetMethod]
            static IEnumerable<MethodBase> TargetMethods()
            {
                yield return AccessTools.Method(typeof(UIReplicatorWindow), "RefreshRecipeIcons");
                yield return AccessTools.Method(typeof(UIReplicatorWindow), "TestMouseRecipeIndex");
                yield return AccessTools.Method(typeof(UIReplicatorWindow), "SetSelectedRecipeIndex");
                yield return AccessTools.Method(typeof(UIReplicatorWindow), "SetSelectedRecipe");
                yield return AccessTools.Method(typeof(UIReplicatorWindow), "_OnInit");
                yield return AccessTools.Method(typeof(UIReplicatorWindow), "RepositionQueueText");
                yield return AccessTools.Method(typeof(UIReplicatorWindow), "RefreshQueueIcons");
                //yield return AccessTools.Method(typeof(UIReplicatorWindow), "_OnCreate");
                yield return AccessTools.Method(typeof(UIReplicatorWindow), "TestMouseQueueIndex");
                yield return AccessTools.Method(typeof(UIRecipePicker), "_OnUpdate");
                yield return AccessTools.Method(typeof(UIRecipePicker), "RefreshIcons");
                yield return AccessTools.Method(typeof(UIRecipePicker), "TestMouseIndex");
                //yield return AccessTools.Method(typeof(UIItemPicker), "_OnUpdate");
                //yield return AccessTools.Method(typeof(UIItemPicker), "RefreshIcons");
                //yield return AccessTools.Method(typeof(UIItemPicker), "TestMouseIndex");
            }

            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldc_I4_S && (sbyte)codes[i].operand == 12)
                    {
                        codes[i].operand = 17;// Convert.ToInt32(colCount);
                    }
                }
                return codes.AsEnumerable();
            }
        }

        //列数の調整一括処理_float
        [HarmonyPatch]
        class Transpiler_replace_12to15_float
        {
            [HarmonyTargetMethod]
            static IEnumerable<MethodBase> TargetMethods()
            {
                yield return AccessTools.Method(typeof(UIReplicatorWindow), "SetMaterialProps");
                yield return AccessTools.Method(typeof(UIRecipePicker), "SetMaterialProps");
                //yield return AccessTools.Method(typeof(UIItemPicker), "SetMaterialProps");
            }

            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    var f = (codes[i].operand as float?);
                    if (codes[i].opcode == OpCodes.Ldc_R4 && f.HasValue && f.Value == 12f)
                    {
                        codes[i].operand = 17f;// Convert.ToSingle(colCount);
                    }
                }
                return codes.AsEnumerable();
            }
        }




        //UIReplicatorWindowレシピグリッド横幅の修正
        //[HarmonyPatch(typeof(UIReplicatorWindow), "_OnInit")]
        public static class UIReplicatorWindow_OnInit_PostPatch
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                if (!resized2)
                {
                    //レシピグリッド
                    ref RectTransform windowRect = ref AccessTools.FieldRefAccess<UIReplicatorWindow, RectTransform>(UIRoot.instance.uiGame.replicator, "windowRect");//670 : 811
                    windowRect.sizeDelta = new Vector2(windowRect.sizeDelta.x + 46 * 5, windowRect.sizeDelta.y);
                    ref RectTransform recipeGroup = ref AccessTools.FieldRefAccess<UIReplicatorWindow, RectTransform>(UIRoot.instance.uiGame.replicator, "recipeGroup"); // 552 : 322
                    recipeGroup.sizeDelta = new Vector2(recipeGroup.sizeDelta.x + 46 * 5, recipeGroup.sizeDelta.y);
                    //キューグリッド
                    //GameObject.Find("UI Root/Overlay Canvas/In Game/Windows/Replicator Window/queue-group").GetComponentInChildren<RectTransform>().sizeDelta = new Vector2(690, 46);
                    resized2 = true;


                }

            }

        }

        //UIRecipePickerレシピグリッド横幅の修正
        //[HarmonyPatch(typeof(UIRecipePicker), "_OnInit")]
        public static class UIRecipePicker1_OnInit_PostPatch
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                if (!resized3)
                {
                    ref RectTransform pickerTrans = ref AccessTools.FieldRefAccess<UIRecipePicker, RectTransform>(UIRoot.instance.uiGame.recipePicker, "pickerTrans");
                    pickerTrans.sizeDelta = new Vector2(pickerTrans.sizeDelta.x + 46 * 5, pickerTrans.sizeDelta.y);
                    //ref RectTransform recipeGroup = ref AccessTools.FieldRefAccess<UIAssemblerWindow, RectTransform>(UIRoot.instance.uiGame.assemblerWindow, "recipeGroup"); // 552 : 322
                    //recipeGroup.sizeDelta = new Vector2(recipeGroup.sizeDelta.x + 46 * 5, recipeGroup.sizeDelta.y);


                    resized3 = true;
                }

            }

        }

        //


    }



    public class LogManager
    {
        public static ManualLogSource Logger;
    }

}