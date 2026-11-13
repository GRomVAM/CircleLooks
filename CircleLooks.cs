using System;
using System.Collections.Generic;
using UnityEngine;

namespace GRomVAM
{

    public class CircleLooks
        : MVRScript
    {

        private readonly UIHelper _uiHelper = new UIHelper(nameof(CircleLooks));

        private readonly Dictionary<string, LooksetItem> _looksets = new Dictionary<string, LooksetItem>();
        private Atom _activePerson = null;
        private LooksetItem _activeLookset = null;

        private JSONStorableStringChooser _looksetChooser;
        private JSONStorableStringChooser _personChooser;
        private JSONStorableStringChooser _hotkeyNextLookChooser;
        private JSONStorableStringChooser _hotkeyPrevLookChooser;
        private JSONStorableStringChooser _hotkeyCycleLooksetChooser;
        private JSONStorableStringChooser _hotkeyCyclePersonChooser;

        private const string _NONE_PLACEHOLDER = "—empty—";


        /* init */


        public override void Init()
        {
            try
            {
                SuppVamIO.UIHelper = _uiHelper;
                SuppVamAtom.UIHelper = _uiHelper;

                //--- UI SETUP

                _looksetChooser = new JSONStorableStringChooser(
                "ActiveLookset", new List<string>(), "", "Active\nLookset", OnActiveLooksetChanged);
                RegisterStringChooser(_looksetChooser);
                CreatePopup(_looksetChooser);
                CreateButton("Refresh Looksets").button.onClick.AddListener(RefreshLooksetsData);

                _personChooser = new JSONStorableStringChooser(
                "TargetPerson", new List<string>(), "", "Target\nPerson", OnTargetPersonChanged);
                RegisterStringChooser(_personChooser);
                CreatePopup(_personChooser);
                CreateButton("Refresh Scene Persons").button.onClick.AddListener(RefreshScenePersonsData);

                CreateButton("Next\nLook").button.onClick.AddListener(NextLook);
                CreateButton("Prev\nLook").button.onClick.AddListener(PrevLook);

                string descText1 =
                    "<b>CircleLooks</b> — quick appearance preset switcher.\n\n" +
                    "Place <b>.vap</b> presets into subfolders inside:\n" +
                    "<i>Custom/Atom/Person/Appearance/</i>\n\n" +
                    "Each subfolder acts as a separate <b>Lookset</b>.\n\n" +
                    "Select a <b>Target Person</b> and cycle looks via buttons, hotkeys, or VaM triggers.";

                var descField1 = CreateTextField(new JSONStorableString("PluginDescription", descText1));
                if (descField1 != null)
                {
                    var layout1 = descField1.GetComponent<UnityEngine.UI.LayoutElement>();
                    if (layout1 != null)
                        layout1.preferredHeight = 400f;
                }

                var availableKeys1 = new List<string> {
                    "None", "K", "L", "O", "P", "N", "M", "U", "I", "F1", "F2", "F3", "F4", "F5"
                };

                _hotkeyNextLookChooser = new JSONStorableStringChooser(
                    "HotkeyNextLook", availableKeys1, "L", "Next\nLook");
                RegisterStringChooser(_hotkeyNextLookChooser);
                CreatePopup(_hotkeyNextLookChooser, true);
                RegisterAction(new JSONStorableAction("NextLook", NextLook));

                _hotkeyPrevLookChooser = new JSONStorableStringChooser(
                    "HotkeyPrevLook", availableKeys1, "K", "Prev\nLook");
                RegisterStringChooser(_hotkeyPrevLookChooser);
                CreatePopup(_hotkeyPrevLookChooser, true);
                RegisterAction(new JSONStorableAction("PrevLook", PrevLook));

                _hotkeyCycleLooksetChooser = new JSONStorableStringChooser(
                    "HotkeyCycleLookset", availableKeys1, "O", "Cycle\nLookset");
                RegisterStringChooser(_hotkeyCycleLooksetChooser);
                CreatePopup(_hotkeyCycleLooksetChooser, true);
                RegisterAction(new JSONStorableAction("CycleLookset", CycleLookset));

                _hotkeyCyclePersonChooser = new JSONStorableStringChooser(
                    "HotkeyCyclePerson", availableKeys1, "P", "Cycle\nPerson");
                RegisterStringChooser(_hotkeyCyclePersonChooser);
                CreatePopup(_hotkeyCyclePersonChooser, true);
                RegisterAction(new JSONStorableAction("CyclePersons", CyclePersons));

                //--- COMPONENTS SETUP

                RefreshLooksetsData();
                RefreshScenePersonsData();
                SuperController.singleton.onSceneLoadedHandlers += OnSceneLoaded;

                //_uiHelper.OutMessage("Init() Successfully.");
            }
            catch (Exception e)
            {
                _uiHelper.OutError($"Init() Error: {e}");
            }
        }


        /* VAM events */


        public void OnSceneLoaded()
        {
            try
            {
                RefreshLooksetsData();
                RefreshScenePersonsData();

                //_uiHelper.OutMessage("OnSceneLoaded() Successfully.");
            }
            catch (Exception e)
            {
                _uiHelper.OutError($"OnSceneLoaded() Error: {e}");
            }
        }


        public void Update()
        {
            try
            {
                if (!_uiHelper.CheckAvailabilityUpdate())
                    return;

                if (_uiHelper.TestHotkey(_hotkeyNextLookChooser))
                    NextLook();
                if (_uiHelper.TestHotkey(_hotkeyPrevLookChooser))
                    PrevLook();
                if (_uiHelper.TestHotkey(_hotkeyCycleLooksetChooser))
                    CycleLookset();
                if (_uiHelper.TestHotkey(_hotkeyCyclePersonChooser))
                    CyclePersons();
            }
            catch (Exception e)
            {
                _uiHelper.OutError($"Update() Error: {e}");
            }
        }


        public void OnDestroy()
        {
            try
            {
                SuppVamAtom.StopAllBlinks(this);
                if (SuperController.singleton != null)
                    SuperController.singleton.onSceneLoadedHandlers -= OnSceneLoaded;
                //_uiHelper.OutMessage("OnDestroy() Successfully.");
                _uiHelper.Dispose();
            }
            catch (Exception e)
            {
                _uiHelper.OutError($"OnDestroy() Error: {e}");
            }
        }


        /* events */


        public void OnActiveLooksetChanged(
            string val)
        {
            if (string.IsNullOrEmpty(val) || val == _NONE_PLACEHOLDER)
            {
                _activeLookset = null;
                return;
            }
            try
            {
                LooksetItem lookset1;
                if (_looksets.TryGetValue(val, out lookset1) && lookset1 != null)
                {
                    _activeLookset = lookset1;
                    _activeLookset.CurrentIndex = -1;
                    _uiHelper.OutMessage($"Selected lookset: '{val}'", true);
                }
                else
                {
                    _activeLookset = null;
                }
            }
            catch (Exception e)
            {
                _uiHelper.OutError($"OnActiveLooksetChanged() Error: {e.Message}");
            }
        }


        public void OnTargetPersonChanged(
            string val)
        {
            if (string.IsNullOrEmpty(val) || val == _NONE_PLACEHOLDER)
            {
                _activePerson = null;
                return;
            }
            try
            {
                _activePerson = SuppVamAtom.FindAtomByUid(val);
                if (_activePerson != null)
                {
                    _uiHelper.OutMessage($"Selected person: '{val}'", true);
                    SuppVamAtom.BlinkAtom(this, _activePerson);
                }                
            }
            catch (Exception e)
            {
                _uiHelper.OutError($"OnTargetPersonChanged() Error: {e.Message}");
            }
        }



        /* methods */


        public void RefreshLooksetsData()
        {
            try
            {
                _looksets.Clear();

                var folders1 = SuppVamIO.GetFolders(SuppVamIO.AppearancePath);
                var choices1 = new List<string>();

                if (folders1 != null)
                {
                    Array.Sort(folders1, StringComparer.OrdinalIgnoreCase);
                    for (int i1 = 0; i1 < folders1.Length; i1++)
                    {
                        string folder1 = folders1[i1];
                        string folderPath1 = $"{SuppVamIO.AppearancePath}/{folder1}";
                        var files1 = SuperController.singleton?.GetFilesAtPath(folderPath1, "*.vap");
                        if (files1 != null && files1.Length > 0)
                        {
                            for (int i2 = 0; i2 < files1.Length; i2++)
                                files1[i2] = SuppVamIO.FixSlashes(files1[i2]);

                            Array.Sort(files1, StringComparer.OrdinalIgnoreCase);
                            int count1 = files1.Length;
                            var name1 = $"{folder1} ({count1})";

                            _looksets[name1] = new LooksetItem
                            {
                                Name = name1,
                                Looks = files1
                            };
                            choices1.Add(name1);
                        }
                    }
                }

                if (choices1.Count == 0)
                {
                    choices1.Add(_NONE_PLACEHOLDER);
                    _looksetChooser.choices = choices1;
                    _looksetChooser.val = _NONE_PLACEHOLDER;
                    _uiHelper.OutMessage($"No lookset folders found in '{SuppVamIO.AppearancePath}'", true, Color.yellow);
                }
                else
                {
                    _looksetChooser.choices = choices1;
                    _looksetChooser.val = choices1[0];
                    _uiHelper.OutMessage($"Successfully found {_looksets.Count} looksets.", true);
                }
            }
            catch (Exception e)
            {
                _uiHelper.OutError($"RefreshLooksetsData() Error: {e.Message}");
            }
        }


        public void RefreshScenePersonsData()
        {
            try
            {
                var persons1 = SuppVamAtom.GetAllPersonsOnScene();
                var choices1 = new List<string>();

                if (persons1 != null && persons1.Length > 0)
                {
                    Array.Sort(persons1, StringComparer.OrdinalIgnoreCase);
                    choices1.AddRange(persons1);
                }

                if (choices1.Count == 0)
                {
                    choices1.Add(_NONE_PLACEHOLDER);
                    _personChooser.choices = choices1;
                    _personChooser.val = _NONE_PLACEHOLDER;
                    _uiHelper.OutMessage("No Person atoms found on scene.", true, Color.yellow);
                }
                else
                {
                    _personChooser.choices = choices1;
                    _personChooser.val = choices1[0];
                    _uiHelper.OutMessage($"Successfully found {choices1.Count} persons.", true);
                }
            }
            catch (Exception e)
            {
                _uiHelper.OutError($"RefreshScenePersonsData() Error: {e.Message}");
            }
        }


        void NextLook()
        {
            ApplyLook(1);
        }


        void PrevLook()
        {
            ApplyLook(-1);
        }


        void CycleLookset()
        {
            CycleChooser(_looksetChooser);
        }


        void CyclePersons()
        {
            CycleChooser(_personChooser);
        }


        public void ApplyLook(
            int direction)
        {
            if (SuperController.singleton != null && SuperController.singleton.isLoading)
            {
                _uiHelper.OutMessage("Engine is busy loading assets. Please wait...", true, Color.yellow);
                return;
            }
            if (_activePerson == null || _activePerson.destroyed)
            {
                _uiHelper.OutMessage("Target person is not selected or destroyed.", true, Color.yellow);
                return;
            }
            if (_activeLookset == null || _activeLookset.Count == 0)
            {
                _uiHelper.OutMessage("Active lookset is not selected or empty.", true, Color.yellow);
                return;
            }
            try
            {
                if (direction >= 0)
                    _activeLookset.Next();
                else
                    _activeLookset.Prev();

                string lookPath1 = _activeLookset.GetActive();
                if (string.IsNullOrEmpty(lookPath1))
                    return;

                if (SuppVamAtom.LoadAppearancePreset(_activePerson, lookPath1, false))
                    _uiHelper.OutMessage($"{_activeLookset.Name} : {_activeLookset.CurrentIndex + 1}", true);
            }
            catch (Exception e)
            {
                _uiHelper.OutError($"ApplyLook() Error: {e.Message}", true);
            }
        }


        public void CycleChooser(
            JSONStorableStringChooser chooser1)
        {
            if (chooser1 == null)
                return;
            try
            {
                var choices1 = chooser1.choices;
                if (choices1 == null || choices1.Count <= 1)
                    return;
                int current1 = choices1.IndexOf(chooser1.val);
                int next1 = (current1 + 1) % choices1.Count;
                chooser1.val = choices1[next1];
            }
            catch (Exception e)
            {
                _uiHelper.OutError($"CycleChooser() [Param: {chooser1.name}] Error: {e.Message}");
            }
        }

    }





    class LooksetItem
    {
        public string Name { get; set; }
        public string[] Looks { get; set; }
        public int CurrentIndex { get; set; } = -1;
        public int Count => Looks?.Length ?? 0;

        public void Next()
        {
            if (Count == 0)
                return;
            CurrentIndex = (CurrentIndex + 1) % Count;
        }

        public void Prev()
        {
            if (Count == 0)
                return;
            CurrentIndex = (CurrentIndex <= 0)
                ? Count - 1
                : CurrentIndex - 1;
        }

        public string GetActive()
        {
            return (Count == 0 || CurrentIndex < 0 || CurrentIndex >= Count)
                ? null : Looks[CurrentIndex];
        }
    }

}