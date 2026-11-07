using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CircleLooks
{

    public class CircleLooks
        : MVRScript
    {

        private Dictionary<string, LooksetItem> _looksets = null;

        private Atom _targetAtomObject = null;
        private string _lockedPersonUid = "";
        private bool _isTargetLoading = false;

        private JSONStorableStringChooser _looksetChooser;
        private JSONStorableStringChooser _personChooser;
        private JSONStorableStringChooser _hotkeyNextLookChooser;
        private JSONStorableStringChooser _hotkeyPrevLookChooser;
        private JSONStorableStringChooser _hotkeyNextLooksetChooser;
        private JSONStorableStringChooser _hotkeyNextPersonChooser;

        private MessageOverlayHelper _messageOverlay = new MessageOverlayHelper();


        /* init */


        public override void Init()
        {
            try
            {

                //--- UI SETUP ---

                // right side

                _personChooser = new JSONStorableStringChooser(
                    "TargetPerson", new List<string>(), "", "Target\nPerson", OnTargetPersonChanged);
                RegisterStringChooser(_personChooser);
                CreatePopup(_personChooser);
                CreateButton("Refresh Scene Persons").button.onClick.AddListener(RefreshScenePersons);

                _looksetChooser = new JSONStorableStringChooser(
                    "ActiveLookset", new List<string>(), "", "Active\nLookset", OnActiveLooksetChanged);
                RegisterStringChooser(_looksetChooser);
                CreatePopup(_looksetChooser);
                CreateButton("Refresh Looksets").button.onClick.AddListener(RefreshLooksets);

                CreateButton("Next\nLook").button.onClick.AddListener(NextLook);
                CreateButton("Prev\nLook").button.onClick.AddListener(PrevLook);
                                
                string descText1 =
                    "Organize your appearance presets into subfolders within 'Saves/Person/Appearance' " +
                    "(e.g., 'Male', 'Female').\n\nEach folder automatically becomes a separate Lookset " +
                    "collection. Select a target character and seamlessly cycle through looks or switch " +
                    "collections using the UI buttons or hotkeys.";

                var descField1 = CreateTextField(new JSONStorableString("PluginDescription", descText1));
                if (descField1 != null)
                {                    
                    var layout1 = descField1.GetComponent<UnityEngine.UI.LayoutElement>();
                    if (layout1 != null)
                        layout1.preferredHeight = 400f;
                }

                // left side

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

                _hotkeyNextLooksetChooser = new JSONStorableStringChooser(
                    "HotkeyNextLookset", availableKeys1, "O", "Next\nLookset");
                RegisterStringChooser(_hotkeyNextLooksetChooser);
                CreatePopup(_hotkeyNextLooksetChooser, true);
                RegisterAction(new JSONStorableAction("CycleLookset", CycleLookset));

                _hotkeyNextPersonChooser = new JSONStorableStringChooser(
                    "HotkeyNextPerson", availableKeys1, "P", "Next\nPerson");
                RegisterStringChooser(_hotkeyNextPersonChooser);
                CreatePopup(_hotkeyNextPersonChooser, true);
                RegisterAction(new JSONStorableAction("CyclePersons", CyclePersons));

                //--- SUBSCRIPTIONS & INITIAL RUN ---

                SuperController.singleton.onSceneLoadedHandlers += OnSceneLoaded;
                RefreshLooksets();
                RefreshScenePersons();

                _messageOverlay.Make();
            }
            catch (Exception ex)
            {
                SuperController.LogError(
                    $"[CircleLooks].Init() Error: {ex}");
            }
        }


        /* vam events */


        public void OnSceneLoaded()
        {
            _targetAtomObject = null;
            _lockedPersonUid = "";
            RefreshLooksets();
            RefreshScenePersons();
            _outMessage("Scene loaded successfully.", true);
        }


        public void Update()
        {
            if (EventSystem.current != null
                && EventSystem.current.currentSelectedGameObject != null
                && EventSystem.current.currentSelectedGameObject.GetComponent<UnityEngine.UI.InputField>() != null)
                return;

            if (_targetAtomObject != null && !string.IsNullOrEmpty(_lockedPersonUid))
            {
                if (_targetAtomObject.uid != _lockedPersonUid)
                {
                    _lockedPersonUid = _targetAtomObject.uid;
                    if (_personChooser != null)
                        _personChooser.valNoCallback = _lockedPersonUid;
                }
            }

            try
            {
                _testHotkey(_hotkeyPrevLookChooser, PrevLook);
                _testHotkey(_hotkeyNextLookChooser, NextLook);
                _testHotkey(_hotkeyNextLooksetChooser, CycleLookset);
                _testHotkey(_hotkeyNextPersonChooser, CyclePersons);

                _messageOverlay.Mute();
            }
            catch (Exception ex)
            {
                SuperController.LogError(
                    $"[CircleLooks].Update() Error: {ex}");
            }
        }


        public void OnDestroy()
        {
            if (SuperController.singleton != null)
                SuperController.singleton.onSceneLoadedHandlers -= OnSceneLoaded;
            _messageOverlay.Destroy();
        }


        /* events */


        void OnTargetPersonChanged(
            string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                _targetAtomObject = null;
                _lockedPersonUid = "";
                return;
            }
            _targetAtomObject = SuperController.singleton.GetAtomByUid(name);
            if (_targetAtomObject != null)
                _lockedPersonUid = _targetAtomObject.uid;
            _outMessage($"Target Person is \"{name}\"", true);
        }


        void OnActiveLooksetChanged(
            string name)
        {
            _outMessage($"Active Lookset is \"{name}\"", true);
        }


        /* methods */


        void RefreshLooksets()
        {
            var folders1 = _getLooksetFolders("Saves/Person/appearance/");
            _looksets = new Dictionary<string, LooksetItem>();
            foreach (var folder1 in folders1)
            {
                var looks1 = _getLooks(folder1);
                if (looks1?.Length > 0)
                {
                    string name1 = folder1.Contains('/')
                        ? folder1.Substring(folder1.LastIndexOf('/') + 1)
                        : folder1;
                    var value1 = new LooksetItem
                    {
                        Name = $"{name1} ({looks1.Length})",
                        Looks = looks1,
                        CurrentIndex = 0,
                    };
                    var key1 = value1.Name;
                    _looksets.Add(key1, value1);
                }
            }
            if (_looksetChooser != null)
            {
                var activeChoices1 = _looksets.Keys.ToList();
                _looksetChooser.choices = activeChoices1;
                _looksetChooser.val = activeChoices1.Count > 0
                    ? activeChoices1.First() : "";
            }
            _outMessage($"Total looksets loaded: \"{_looksetChooser.choices.Count}\"", true);
        }


        void RefreshScenePersons()
        {
            var persons1 = new List<string>();
            var atoms1 = SuperController.singleton.GetAtoms();
            var previousSelection1 = _personChooser != null
                ? _personChooser.val : "";

            foreach (var atom1 in atoms1)
                if (atom1.type == "Person")
                    persons1.Add(atom1.uid);

            if (_personChooser != null)
            {
                _personChooser.choices = persons1;
                if (persons1.Contains(previousSelection1))
                    _personChooser.val = previousSelection1;
                else if (_targetAtomObject != null && persons1.Contains(_targetAtomObject.uid))
                    _personChooser.val = _targetAtomObject.uid;
                else if (persons1.Count > 0)
                    _personChooser.val = persons1[0];
                else
                    _personChooser.val = "";
            }
            _outMessage($"Total persons found: \"{_personChooser.choices.Count}\"", true);
        }


        void NextLook()
        {
            MoveIndex(1);
        }


        void PrevLook()
        {
            MoveIndex(-1);
        }


        void CycleLookset()
        {
            _cycler(_looksetChooser);
        }


        void CyclePersons()
        {
            _cycler(_personChooser);
        }


        void MoveIndex(
            int direction)
        {
            if (_isTargetLoading)
            {
                _outMessage("The look loading has not been completed yet...", true, Color.red);
                return;
            }

            if (_looksetChooser == null
                || string.IsNullOrEmpty(_looksetChooser.val)
                || _looksets == null)
                return;

            var lookset1 = _looksets[_looksetChooser.val];
            if (lookset1.Count == 0)
                return;

            if (direction == 1)
                lookset1.Next();
            else if (direction == -1)
                lookset1.Prev();
            ApplyLook(lookset1);
        }


        void ApplyLook(
            LooksetItem lookset)
        {
            if (_isTargetLoading)
            {
                _outMessage("The look loading has not been completed yet...", true, Color.red);
                return;
            }

            if (_targetAtomObject == null)
            {
                if (_personChooser == null || string.IsNullOrEmpty(_personChooser.val))
                    return;
                _targetAtomObject = SuperController.singleton.GetAtomByUid(_personChooser.val);
                if (_targetAtomObject != null)
                    _lockedPersonUid = _targetAtomObject.uid;
            }
            if (_targetAtomObject == null)
                return;

            var look1 = lookset.GetActive();
            if (look1 == null)
                return;

            _isTargetLoading = true;
            _targetAtomObject.LoadAppearancePreset(look1);
            StartCoroutine(WaitForLookLoadRoutine());
        }


        /* functions */


        private IEnumerator WaitForLookLoadRoutine()
        {
            yield return null;
            yield return null;
            yield return null;
            while (SuperController.singleton.isLoading)
                yield return null;
            _isTargetLoading = false;
            _outMessage("Look applied successfully.");
        }



        /* privates */


        private void _outMessage(
            string message,
            bool showInView = false,
            Color? textColor = null,
            float? delay = null)
        {
            if (showInView)
            {
                _messageOverlay.Show(message, textColor, delay);
            }
            SuperController.LogMessage(
                $"[CircleLooks] {message}");
        }


        private void _testHotkey(
            JSONStorableStringChooser chooser,
            Action action)
        {
            if (chooser?.val != "None"
                && Input.GetKeyDown((KeyCode)Enum.Parse(typeof(KeyCode), chooser.val)))
                action();
        }


        private static string[] _getLooksetFolders(
            string path)
        {
            var path1 = SuperController.singleton.NormalizePath(path);
            var folders1 = SuperController.singleton.GetDirectoriesAtPath(path1);
            return folders1.Select(x => x.Replace('\\', '/')).ToArray();
        }


        private static string[] _getLooks(
            string path)
        {
            var looks1 = new List<string>();
            try
            {
                var path1 = SuperController.singleton.NormalizePath(path);
                var files1 = SuperController.singleton.GetFilesAtPath(path1, "*.json");
                if (files1?.Length > 0)
                    foreach (string file1 in files1)
                        looks1.Add(file1.Replace("\\", "/"));
            }
            catch (Exception ex)
            {
                SuperController.LogError(
                    $"[CircleLooks]._getLooks(\"{path}\") Error: {ex}");
            }
            return looks1.ToArray();
        }


        private void _cycler(
            JSONStorableStringChooser chooser)
        {
            if (chooser == null || chooser.choices.Count < 2)
                return;
            int current1 = chooser.choices.IndexOf(chooser.val);
            int next1 = (current1 + 1) % chooser.choices.Count;
            chooser.val = chooser.choices[next1];
        }





        class LooksetItem
        {
            public string Name { get; set; }
            public string[] Looks { get; set; }
            public int CurrentIndex { get; set; } = 0;
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
                CurrentIndex = (CurrentIndex - 1 + Count) % Count;
            }

            public string GetActive()
            {
                return (Count == 0 || CurrentIndex < 0 || CurrentIndex >= Count)
                    ? null
                    : Looks[CurrentIndex];
            }
        }



        public sealed class MessageOverlayHelper
        {
            public const float MSG_ALPHA = 0.5f;
            public const float MSG_DELAY = 2f;

            private GameObject _msgOverlayGO = null;
            private UnityEngine.UI.Text _msgOverlayText = null;
            private float _msgOverlayAlfa = MSG_ALPHA;
            private float _msgOverlayTimer = 0f;
            private Color _baseColor = Color.green;

            public void Make(
                Color? textColor = null,
                float defaultAlpha = MSG_ALPHA)
            {
                _baseColor = textColor ?? Color.green;
                _msgOverlayAlfa = defaultAlpha;

                _msgOverlayGO = new GameObject("CircleLooksOverlay");
                if (SuperController.singleton.MonitorCenterCamera != null)
                    _msgOverlayGO.transform.SetParent(
                        SuperController.singleton.MonitorCenterCamera.transform, false);

                _msgOverlayGO.transform.localPosition = new Vector3(0f, 0f, 1f);

                var canvas1 = _msgOverlayGO.AddComponent<Canvas>();
                canvas1.renderMode = RenderMode.WorldSpace;
                _msgOverlayGO.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 100;

                var canvasRect1 = _msgOverlayGO.GetComponent<RectTransform>();
                canvasRect1.sizeDelta = new Vector2(800f, 200f);
                canvasRect1.localScale = new Vector3(0.003f, 0.003f, 0.003f);

                var text1 = new GameObject("OverlayText");
                text1.transform.SetParent(_msgOverlayGO.transform, false);

                _msgOverlayText = text1.AddComponent<UnityEngine.UI.Text>();
                _msgOverlayText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                _msgOverlayText.fontSize = 32;
                _msgOverlayText.alignment = TextAnchor.MiddleCenter;

                var startColor = _baseColor;
                startColor.a = _msgOverlayAlfa;
                _msgOverlayText.color = startColor;

                var textRect1 = text1.GetComponent<RectTransform>();
                textRect1.anchoredPosition = Vector2.zero;
                textRect1.sizeDelta = new Vector2(800f, 200f);

                _msgOverlayGO.SetActive(false);
            }

            public void Mute()
            {
                if (_msgOverlayGO != null && _msgOverlayTimer > 0f)
                {
                    _msgOverlayTimer -= Time.unscaledDeltaTime;
                    if (_msgOverlayTimer <= 0f)
                    {
                        _msgOverlayGO.SetActive(false);
                    }
                    else if (_msgOverlayText != null && _msgOverlayTimer < 1f)
                    {
                        var color1 = _msgOverlayText.color;
                        color1.a = _msgOverlayTimer * _msgOverlayAlfa;
                        _msgOverlayText.color = color1;
                    }
                }
            }

            public void Show(
                string message,
                Color? textColor = null,
                float? delay = null)
            {
                if (_msgOverlayText != null && _msgOverlayGO != null)
                {
                    _msgOverlayText.text = message;
                    var color1 = textColor ?? _baseColor;
                    color1.a = _msgOverlayAlfa;
                    _msgOverlayText.color = color1;
                    _msgOverlayGO.SetActive(true);
                    _msgOverlayTimer = delay ?? MSG_DELAY;
                }
            }

            public void Destroy()
            {
                if (_msgOverlayGO != null)
                    UnityEngine.Object.Destroy(_msgOverlayGO);
            }
        }

    }

}
