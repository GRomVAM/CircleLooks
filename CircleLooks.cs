using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CircleLooks
{

    /// <summary>
    /// Main Virt-A-Mate plugin script that enables seamless cyclic switching of character appearance presets (looks)
    /// organized into subfolder collections (looksets).
    /// Provides UI controls, configurable keyboard hotkeys, asynchronous loading locks, and automatic UID restoration
    /// to preserve scene logic and links when loading external presets.
    /// </summary>
    public class CircleLooks
        : MVRScript
    {

        /// <summary>
        /// Dictionary for fast access to lookset objects by their formatted folder names.
        /// Key is the folder name with count (e.g., "Cyberpunk (12)"), value is the LooksetItem instance.
        /// </summary>
        private Dictionary<string, LooksetItem> _looksetMap = new Dictionary<string, LooksetItem>();

        private Atom _lookedPerson = null;
        private string _lockedPersonUid = "";
        private bool _isTargetLoading = false;

        private UIHelper _uiHelper = new UIHelper(nameof(CircleLooks));

        private JSONStorableStringChooser _looksetChooser;
        private JSONStorableStringChooser _personChooser;
        private JSONStorableStringChooser _hotkeyNextLookChooser;
        private JSONStorableStringChooser _hotkeyPrevLookChooser;
        private JSONStorableStringChooser _hotkeyCycleLooksetChooser;
        private JSONStorableStringChooser _hotkeyCyclePersonChooser;


        /* init */


        /// <summary>
        /// Called by the Virt-A-Mate runtime once during plugin initialization.
        /// Used to create parameters, register storables, and perform initial logic setup.
        /// </summary>
        public override void Init()
        {
            try
            {
                SuppVamIO.UIHelper = _uiHelper;
                SuppVamAtom.UIHelper = _uiHelper;

                _makeUI();

                //RefreshScenePersonsData();
                //RefreshLooksetsData();

                SuperController.singleton.onSceneLoadedHandlers += OnSceneLoaded;

                //_uiHelper.OutMessage("Init()");
            }
            catch (Exception e)
            {
                _uiHelper.OutError($"Init() Error: {e}");
            }
        }

        /* VAM events */


        /// <summary>
        /// Triggered when a new scene finishes loading from the game menu.
        /// Automatically synchronizes the lists of scene characters and available looksets.
        /// </summary>
        public void OnSceneLoaded()
        {
            try
            {
                RefreshScenePersonsData();
                RefreshLooksetsData();
                //_uiHelper.OutMessage($"OnSceneLoaded()");
            }
            catch (Exception e)
            {
                _uiHelper.OutError($"OnSceneLoaded() Error: {e}");
            }
        }


        /// <summary>
        /// Main Unity game loop executed every frame.
        /// Handles safe interception and processing of keyboard hotkeys and message overlay fade timers.
        /// </summary>
        public void Update()
        {
            try
            {
                _uiHelper?.MuteMessage();

                if (SuperController.singleton?.isLoading ?? true)
                    return;

                var eventSystem1 = UnityEngine.EventSystems.EventSystem.current;
                if (eventSystem1 != null
                    && eventSystem1.currentSelectedGameObject != null
                    && eventSystem1.currentSelectedGameObject.GetComponent<UnityEngine.UI.InputField>() != null)
                    return;

                _uiHelper.TestHotkey(_hotkeyNextLookChooser, NextLook);
                _uiHelper.TestHotkey(_hotkeyPrevLookChooser, PrevLook);
                _uiHelper.TestHotkey(_hotkeyCycleLooksetChooser, CycleLookset);
                _uiHelper.TestHotkey(_hotkeyCyclePersonChooser, CyclePersons);
            }
            catch (Exception e)
            {
                _uiHelper.OutError($"Update() Error: {e}");
            }
        }



        /// <summary>
        /// Called when the plugin is removed from the tab or when the game closes.
        /// Cleans up UI resources and unsubscribes from global game events.
        /// </summary>
        public void OnDestroy()
        {
            try
            {
                _uiHelper.Dispose();
                if (SuperController.singleton != null)
                    SuperController.singleton.onSceneLoadedHandlers -= OnSceneLoaded;
                //_uiHelper.OutMessage($"OnDestroy()");
            }
            catch (Exception e)
            {
                _uiHelper.OutError($"OnDestroy() Error: {e}");
            }
        }


        /* plugin events */


        /// <summary>
        /// Triggered automatically when the user changes the active lookset (collection folder) in the UI.
        /// Resets the internal index of the selected collection to its initial value.
        /// </summary>
        /// <param name="val">The new string name of the selected lookset (e.g., "Cyberpunk (12)").</param>
        void OnActiveLooksetChanged(
            string val)
        {
            if (string.IsNullOrEmpty(val) || !_looksetMap.ContainsKey(val))
                return;
            try
            {
                var lookset1 = _looksetMap[val];
                if (lookset1 != null)
                {
                    lookset1.CurrentIndex = 0;
                    _uiHelper.OutMessage($"Selected lookset '{val}'.", true);
                }
            }
            catch (Exception e)
            {
                _uiHelper.OutError($"OnActiveLooksetChanged() Error: {e.Message}");
            }
        }


        /// <summary>
        /// Triggered automatically when the user selects a different target character on the scene in the UI.
        /// Displays a visual notification confirming the focus switch to the new character.
        /// </summary>
        /// <param name="val">The unique identifier (UID) of the newly selected character atom.</param>
        void OnTargetPersonChanged(
            string val)
        {
            if (string.IsNullOrEmpty(val) || val == "None")
                return;
            try
            {
                _uiHelper.OutMessage($"Selected person: '{val}'", true);
            }
            catch (Exception e)
            {
                _uiHelper.OutError($"OnTargetPersonChanged() Error: {e.Message}");
            }
        }



        /* methods */


        /// <summary>
        /// Scans the current scene for characters of type "Person" 
        /// and updates the available choices in the target person dropdown list.
        /// </summary>
        void RefreshScenePersonsData()
        {
            try
            {
                var persons1 = SuppVamAtom.GetAllPersonsOnScene();
                if (persons1 == null || persons1.Length == 0)
                {
                    _personChooser.choices = new List<string>();
                    _personChooser.valNoCallback = "";
                    //_uiHelper.OutMessage("RefreshScenePersonsData() No Person atoms found on scene.");
                    return;
                }
                var choicesList1 = new List<string>(persons1);
                _personChooser.choices = choicesList1;
                if (string.IsNullOrEmpty(_personChooser.val) || !choicesList1.Contains(_personChooser.val))
                    _personChooser.val = choicesList1[0];
                //_uiHelper.OutMessage($"RefreshScenePersonsData() Successfully cached {choicesList1.Count} persons.");
            }
            catch (Exception e)
            {
                _uiHelper.OutError($"RefreshScenePersonsData() Error: {e.Message}");
            }
        }


        /// <summary>
        /// Performs a full scan of the "Saves/Person/appearance" directory on disk,
        /// populates the _looksetMap dictionary with collection objects, and prepares data for the UI lookset list.
        /// </summary>
        void RefreshLooksetsData()
        {
            try
            {
                _looksetMap.Clear();

                var folders1 = SuppVamIO.GetAllLooksets();
                if (folders1 == null || folders1.Length == 0)
                {
                    _uiHelper.OutMessage("RefreshLooksetsData() No lookset folders found in Saves/Person/appearance");
                    return;
                }

                for (int i1 = 0; i1 < folders1.Length; i1++)
                {
                    var folder1 = folders1[i1];
                    var path1 = $"Saves/Person/appearance/{folder1}/";
                    var looks1 = SuperController.singleton?.GetFilesAtPath(path1, "*.json");
                    var count1 = looks1?.Length ?? 0;
                    if (count1 < 1)
                        continue;
                    var name1 = $"{folder1} ({count1})";
                    var lookset1 = new LooksetItem
                    {
                        Name = name1,
                        Looks = looks1,
                        CurrentIndex = 0,
                    };
                    _looksetMap.Add(name1, lookset1);
                }

                var looksetChoices1 = new List<string>(_looksetMap.Keys);
                _looksetChooser.choices = looksetChoices1;
                if (looksetChoices1.Count > 0
                    && (string.IsNullOrEmpty(_looksetChooser.val) || !looksetChoices1.Contains(_looksetChooser.val)))
                    _looksetChooser.val = looksetChoices1[0];

                //_uiHelper.OutMessage($"RefreshLooksetsData() Successfully scanned {_looksetMap.Count} lookset collections.");
            }
            catch (Exception e)
            {
                _uiHelper.OutError($"RefreshLooksetsData() Error: {e.Message}");
            }
        }


        /// <summary>
        /// Advances the index of the current lookset forward and applies the next look in order to the target character.
        /// </summary>
        void NextLook()
        {
            _applyLook(1);
        }


        /// <summary>
        /// Moves the index of the current lookset backward and applies the previous look in order to the target character.
        /// </summary>
        void PrevLook()
        {
            _applyLook(-1);
        }


        /// <summary>
        /// Cyclically switches the active lookset in the UI to the next available collection.
        /// </summary>
        void CycleLookset()
        {
            _cycleChooser(_looksetChooser);
        }


        /// <summary>
        /// Cyclically switches the target character on the scene in the UI to the next available person.
        /// </summary>
        void CyclePersons()
        {
            _cycleChooser(_personChooser);
        }


        /* functions */


        /// <summary>
        /// Background coroutine monitoring the asynchronous loading state of the VaM engine.
        /// Ensures input remains locked until loading completes and restores the character's original UID.
        /// </summary>
        /// <returns>An enumerator used by Unity to manage coroutine execution.</returns>
        IEnumerator WaitForLookLoadRoutine()
        {
            // Skip exactly 3 frames. This is a critical Unity interval required by the VaM engine
            // to process the LoadAppearancePreset command, open files, and raise the isLoading flag.
            yield return null;
            yield return null;
            yield return null;

            // Hold the coroutine thread (and plugin input lock) while the game's SuperController reports
            // ongoing disk reads, mesh generation, or clothing texture loading.
            while (SuperController.singleton.isLoading)
                yield return null;

            // --- ANTI-RENAME PROTECTION ---
            // If the atom's UID changed after loading files to a name hardcoded by the preset author in JSON:
            if (_lookedPerson != null && _lookedPerson?.uid != _lockedPersonUid)
            {
                string wrongName1 = _lookedPerson.uid;
                SuperController.singleton.RenameAtom(_lookedPerson, _lockedPersonUid);
                //_uiHelper.OutMessage($"Restored original UID for character. Changed from '{wrongName1}' back to '{_lockedPersonUid}'");
            }

            // Loading and UID restoration are completely finished — safely release the plugin input lock!
            _uiHelper.OutMessage("Look applied successfully.");
            _isTargetLoading = false;
        }


        /* privates */


        /// <summary>
        /// Initializes and registers all Virt-A-Mate UI elements, buttons, popups, and hotkey bindings.
        /// </summary>
        private void _makeUI()
        {
            //--- UI SETUP: CONTROL PANEL (LEFT SIDE) ---

            _personChooser = new JSONStorableStringChooser(
                "TargetPerson", new List<string>(), "", "Target\nPerson", OnTargetPersonChanged);
            RegisterStringChooser(_personChooser);
            CreatePopup(_personChooser);
            CreateButton("Refresh Scene Persons").button.onClick.AddListener(RefreshScenePersonsData);

            _looksetChooser = new JSONStorableStringChooser(
                "ActiveLookset", new List<string>(), "", "Active\nLookset", OnActiveLooksetChanged);
            RegisterStringChooser(_looksetChooser);
            CreatePopup(_looksetChooser);
            CreateButton("Refresh Looksets").button.onClick.AddListener(RefreshLooksetsData);

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
                    layout1.preferredHeight = 400f; // Выставляем высоту, чтобы текст не сжимался в UI
            }

            //--- UI SETUP: HOTKEYS (RIGHT SIDE) ---

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
        }


        /// <summary>
        /// Cyclically advances the selected value of a JSONStorableStringChooser to the next available item.
        /// </summary>
        /// <param name="chooser1">The chooser UI component to cycle through.</param>
        private void _cycleChooser(
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
                _uiHelper.OutError($"_cycleChooser() [Param: {chooser1.name}] Error: {e.Message}");
            }
        }


        /// <summary>
        /// Shifts the active look index in the specified direction and applies the appearance preset to the target character.
        /// Locks user input and initiates the background loading monitor routine.
        /// </summary>
        /// <param name="direction">Index shift direction (1 for next look, -1 for previous look).</param>
        private void _applyLook(
            int direction)
        {
            // Guard: if the previous preset has not finished loading, strictly block the click,
            // display a red warning, and hold it on screen
            if (_isTargetLoading)
            {
                _uiHelper.OutMessage("The look loading has not been completed yet...", true, Color.red);
                return;
            }

            if (string.IsNullOrEmpty(_looksetChooser.val))
                return;
            var lookset1 = _looksetMap[_looksetChooser.val];
            if (lookset1 == null)
                return;

            if (direction == 1)
                lookset1.Next();
            else
                lookset1.Prev();

            try
            {
                string uid1 = _personChooser.val;
                if (string.IsNullOrEmpty(uid1) || uid1 == "None")
                {
                    _uiHelper.OutMessage("_applyLook() aborted: No target person selected.");
                    return;
                }

                string look1 = lookset1.GetActive();
                if (string.IsNullOrEmpty(look1))
                    return;

                var person1 = SuppVamAtom.FindAtomByUid(uid1);
                if (person1 == null)
                {
                    _uiHelper.OutError($"_applyLook() failed: Character '{uid1}' not found on scene.");
                    return;
                }

                // Enable input lock: Update and index shifts will ignore commands until complete
                _isTargetLoading = true;
                _lookedPerson = person1;
                _lockedPersonUid = person1.uid;

                // Trigger the official asynchronous VaM method to apply the appearance to the character
                person1.LoadAppearancePreset(look1);
                _uiHelper.OutMessage($"[{lookset1.CurrentIndex + 1}/{lookset1.Count}] Applied look '{look1}' to '{uid1}'");

                // Immediately start the background coroutine to monitor the engine loading status
                StartCoroutine(WaitForLookLoadRoutine());
            }
            catch (Exception e)
            {
                // In case of a critical failure, always release the lock so the plugin does not freeze
                _isTargetLoading = false;
                _uiHelper.OutError($"_applyLook() Error: {e.Message}");
            }
        }





        // ----------------------------
        // internal classes
        // ----------------------------





        /// <summary>
        /// Encapsulates a specific lookset folder (collection of appearance presets) and manages its current index.
        /// </summary>
        class LooksetItem
        {

            /* properties */


            /// <summary>
            /// Display name of the lookset for the UI (e.g., "Female (45)").
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Array of preset paths (relative paths to .json files within this folder).
            /// </summary>
            public string[] Looks { get; set; }

            /// <summary>
            /// Currently selected file index within the array.
            /// </summary>
            public int CurrentIndex { get; set; } = 0;

            /// <summary>
            /// Total number of appearance presets in this folder.
            /// </summary>
            public int Count => Looks?.Length ?? 0;


            /* methods */


            /// <summary>
            /// Advances the current index forward with wrap-around looping.
            /// </summary>
            public void Next()
            {
                if (Count == 0)
                    return;
                CurrentIndex = (CurrentIndex + 1) % Count;
            }

            /// <summary>
            /// Moves the current index backward with wrap-around looping.
            /// </summary>
            public void Prev()
            {
                if (Count == 0)
                    return;
                CurrentIndex = (CurrentIndex - 1 + Count) % Count;
            }


            /* functcions */


            /// <summary>
            /// Retrieves the relative string path to the active .json preset file based on the current index.
            /// </summary>
            /// <returns>The path to the active preset file, or null if the collection is empty or out of bounds.</returns>
            public string GetActive()
            {
                return (Count == 0 || CurrentIndex < 0 || CurrentIndex >= Count)
                    ? null
                    : Looks[CurrentIndex];
            }

        }

    }





    // ========================================================================
    // 
    //      Supports
    // 
    // ========================================================================





    /// <summary>
    /// Helper class for simplified creation, configuration, and management of Virt-A-Mate UI elements and screen message overlays.
    /// </summary>
    public class UIHelper
        : IDisposable
    {

        private GameObject _msgOverlayGO = null;
        private UnityEngine.UI.Text _msgOverlayText = null;
        private float _msgOverlayTimer = 0f;


        /* consts */


        /// <summary>
        /// Default text color for on-screen overlay messages.
        /// </summary>
        public readonly Color DEFAULT_COLOR = Color.green;

        /// <summary>
        /// Default alpha transparency for on-screen overlay messages.
        /// </summary>
        public const float DEFAULT_ALPHA = 0.5f;

        /// <summary>
        /// Default display duration in seconds for on-screen overlay messages.
        /// </summary>
        public const float DEFAULT_DELAY = 2f;


        /* ctor/dtor */


        /// <summary>
        /// Initializes a new instance of the <see cref="UIHelper"/> class and constructs the 3D message overlay canvas.
        /// </summary>
        /// <param name="pluginName">The name of the parent plugin used as a prefix in log messages.</param>
        public UIHelper(
            string pluginName)
        {
            PluginName = pluginName;
            _makeMessageOverlay();
        }


        /// <summary>
        /// Releases all Unity UI GameObjects and resources used by the <see cref="UIHelper"/> instance.
        /// </summary>
        public void Dispose()
        {
            _dispose(true);
            GC.SuppressFinalize(this);
        }
        private bool _disposed = false;


        /// <summary>
        /// Protected implementation of the Dispose pattern to safely destroy managed Unity objects.
        /// </summary>
        /// <param name="disposing">True if called explicitly from <see cref="Dispose"/>; false if called from a finalizer.</param>
        protected virtual void _dispose(
            bool disposing)
        {
            if (_disposed)
                return;
            if (disposing)
                _destroyMessageOverlay();
            _disposed = true;
        }


        /* readonly properties */


        /// <summary>
        /// Gets the name of the associated plugin.
        /// </summary>
        public string PluginName { get; }


        /* methods */


        /// <summary>
        /// Centralized service for outputting informational plugin messages to the system log and optionally to the 3D screen overlay.
        /// </summary>
        /// <param name="text">The message text to display.</param>
        /// <param name="showInUI">Flag indicating whether to force display the message on the user's 3D-View overlay.</param>
        /// <param name="textColor">Optional custom text color for the on-screen overlay.</param>
        /// <param name="delay">Optional custom display duration in seconds before fading out.</param>
        public void OutMessage(
            string text,
            bool showInUI = false,
            Color? textColor = null,
            float? delay = null)
        {
            try
            {
                string formattedMsg1 = $"[{PluginName}] {text}";
                SuperController.LogMessage(formattedMsg1);
                if (showInUI)
                    ShowMessage(text, textColor, delay);
            }
            catch (Exception e)
            {
                SuperController.LogError($"[{PluginName}] UIHelper.OutMessage() Critical Error: {e.Message}");
            }
        }


        /// <summary>
        /// Centralized service for outputting plugin error messages to the system error log and optionally to the 3D screen overlay.
        /// </summary>
        /// <param name="text">The error message text to display.</param>
        /// <param name="showInUI">Flag indicating whether to force display the error on the user's 3D-View overlay.</param>
        /// <param name="textColor">Optional custom text color (defaults to red if null).</param>
        /// <param name="delay">Optional custom display duration in seconds before fading out.</param>
        public void OutError(
            string text,
            bool showInUI = false,
            Color? textColor = null,
            float? delay = null)
        {
            try
            {
                string formattedErr1 = $"[{PluginName}] {text}";
                SuperController.LogError(formattedErr1);
                if (showInUI)
                    ShowMessage(text, textColor ?? Color.red, delay);
            }
            catch (Exception e)
            {
                SuperController.LogError($"[{PluginName}] UIHelper.OutError() Critical Error: {e.Message}");
            }
        }


        /// <summary>
        /// Checks for a physical keyboard key press based on the selected value in a <see cref="JSONStorableStringChooser"/>
        /// and safely invokes the associated action delegate.
        /// </summary>
        /// <param name="chooser">The dropdown chooser instance containing the configured hotkey name.</param>
        /// <param name="action">The action delegate to execute when the hotkey is pressed.</param>
        public void TestHotkey(
            JSONStorableStringChooser chooser,
            Action action)
        {
            if (chooser == null || action == null)
                return;
            try
            {
                if (chooser.val != "None"
                    && Input.GetKeyDown((KeyCode)Enum.Parse(typeof(KeyCode), chooser.val)))
                    action();
            }
            catch (Exception e)
            {
                OutError($"UIHelper.TestHotkey() [Param: {chooser.name}] Error: {e.Message}");
            }
        }


        /// <summary>
        /// Activates the 3D overlay canvas, resets text opacity, and displays the specified message string.
        /// </summary>
        /// <param name="message">The text string to render on the overlay.</param>
        /// <param name="textColor">Optional text color (uses <see cref="DEFAULT_COLOR"/> if null).</param>
        /// <param name="delay">Optional lifetime in seconds (uses <see cref="DEFAULT_DELAY"/> if null).</param>
        public void ShowMessage(
            string message,
            Color? textColor = null,
            float? delay = null)
        {
            if (_msgOverlayText != null && _msgOverlayGO != null)
            {
                _msgOverlayText.text = message;
                var color1 = textColor ?? DEFAULT_COLOR;
                color1.a = DEFAULT_ALPHA;
                _msgOverlayText.color = color1;
                _msgOverlayGO.SetActive(true);
                _msgOverlayTimer = delay ?? DEFAULT_DELAY;
            }
        }


        /// <summary>
        /// Ticks every frame in the plugin's Update loop to decrement the overlay timer and apply a smooth alpha fade-out effect.
        /// </summary>
        public void MuteMessage()
        {
            if (_msgOverlayGO != null && _msgOverlayTimer > 0f)
            {
                _msgOverlayTimer -= Time.unscaledDeltaTime;
                if (_msgOverlayTimer <= 0f)
                    _msgOverlayGO.SetActive(false);
                else if (_msgOverlayText != null && _msgOverlayTimer < 1f)
                {
                    var color1 = _msgOverlayText.color;
                    color1.a = _msgOverlayTimer * DEFAULT_ALPHA;
                    _msgOverlayText.color = color1;
                }
            }
        }


        /* privates */


        /// <summary>
        /// Constructs the WorldSpace UI Canvas and Text components parented to the center monitor camera.
        /// </summary>
        private void _makeMessageOverlay()
        {
            _msgOverlayGO = new GameObject("UIHelper_MessageOverlay");
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
            var text1 = new GameObject("UIHelper_MessageOverlay_Text");
            text1.transform.SetParent(_msgOverlayGO.transform, false);
            _msgOverlayText = text1.AddComponent<UnityEngine.UI.Text>();
            _msgOverlayText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            _msgOverlayText.fontSize = 32;
            _msgOverlayText.alignment = TextAnchor.MiddleCenter;
            var color1 = DEFAULT_COLOR;
            color1.a = DEFAULT_ALPHA;
            _msgOverlayText.color = color1;
            var textRect1 = text1.GetComponent<RectTransform>();
            textRect1.anchoredPosition = Vector2.zero;
            textRect1.sizeDelta = new Vector2(800f, 200f);
            _msgOverlayGO.SetActive(false);
        }


        /// <summary>
        /// Destroys the overlay GameObject if it exists.
        /// </summary>
        private void _destroyMessageOverlay()
        {
            if (_msgOverlayGO != null)
                UnityEngine.Object.Destroy(_msgOverlayGO);
        }


        // SuperController.AlertUser("Hello from my plugin!", () => SuperController.LogMessage("User clicked OK"));
        // SuperController.AlertUser("Reset all settings?",
        //      () => SuperController.LogMessage("Confirmed"),   // OK
        //      () => SuperController.LogMessage("Cancelled"));  // Cancel

    }





    /// <summary>
    /// Вспомогательный класс для безопасной работы с файловой системой и пакетами Virt-A-Mate.
    /// </summary>
    public static class SuppVamIO
    {

        public static UIHelper UIHelper { get; set; } = null;


        /// <summary>
        /// Сканирует корневую директорию внешностей "Saves/Person/appearance" и возвращает имена папок верхнего уровня (лукветов).
        /// </summary>
        /// <returns>Массив строк с именами папок-коллекций, если найдена хотя бы одна папка; в противном случае — null.</returns>
        public static string[] GetAllLooksets()
        {
            try
            {
                const string rootPath1 = "Saves/Person/appearance";

                var dirs1 = SuperController.singleton?.GetDirectoriesAtPath(rootPath1);
                if (dirs1 == null || dirs1.Length == 0)
                    return null;

                var looksetNames1 = new List<string>();
                for (int i1 = 0; i1 < dirs1.Length; i1++)
                {
                    var path1 = _fixSlashes(dirs1[i1]);
                    if (string.IsNullOrEmpty(path1))
                        continue;
                    looksetNames1.Add(_getDirname(path1));
                }
                return looksetNames1.Count > 0
                    ? looksetNames1.ToArray()
                    : null;
            }
            catch (Exception e)
            {
                UIHelper?.OutError($"FileHelper.GetAllLooksets() Error: {e.Message}");
            }
            return null;
        }


        /// <summary>
        /// Сканирует указанную директорию игры и возвращает имена файлов с определенным расширением.
        /// </summary>
        /// <param name="directoryPath">Путь к сканируемой папке относительно корня игры.</param>
        /// <param name="extensionFilter">Фильтр расширения файлов (например, "*.vap").</param>
        /// <returns>Массив имен файлов без расширения, если найден хотя бы один файл; в противном случае — null.</returns>
        public static string[] GetShortFileNames(
            string directoryPath,
            string extensionFilter)
        {
            try
            {
                var fullPaths1 = SuperController.singleton?.GetFilesAtPath(directoryPath, extensionFilter);
                if (fullPaths1 == null || fullPaths1.Length == 0)
                    return null;
                var shortNames1 = new string[fullPaths1.Length];
                for (int i1 = 0; i1 < fullPaths1.Length; i1++)
                {
                    var path1 = fullPaths1[i1];
                    if (string.IsNullOrEmpty(path1))
                        continue;
                    int lastSlash1 = path1.LastIndexOf('/');
                    if (lastSlash1 == -1)
                        lastSlash1 = path1.LastIndexOf('\\');
                    string fileName1 = (lastSlash1 != -1)
                        ? path1.Substring(lastSlash1 + 1)
                        : path1;
                    int lastDot1 = fileName1.LastIndexOf('.');
                    shortNames1[i1] = (lastDot1 != -1)
                        ? fileName1.Substring(0, lastDot1)
                        : fileName1;
                }
                return shortNames1.Length > 0
                    ? shortNames1
                    : null;
            }
            catch (Exception e)
            {
                UIHelper?.OutError($"FileHelper.GetShortFileNames() [Path: {directoryPath}] Error: {e.Message}");
            }
            return null;
        }


        /* privates */


        private static string _fixSlashes(
            string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;
            var s1 = path.Replace('\\', '/');
            return s1.EndsWith("/")
                ? s1.Substring(0, s1.Length - 1)
                : s1;
        }


        /// <summary>
        /// Извлекаем чистое имя папки
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static string _getDirname(
            string path)
        {
            int i1 = path.LastIndexOf('/');
            return i1 != -1
                ? path.Substring(i1 + 1)
                : path;
        }

    }





    /// <summary>
    /// Вспомогательный класс для работы с атомами и иерархией сцены Virt-A-Mate.
    /// </summary>
    public static class SuppVamAtom
    {

        public static UIHelper UIHelper { get; set; } = null;


        /// <summary>
        /// Возвращает список UID всех атомов типа "Person" (персонажей), присутствующих на сцене.
        /// </summary>
        /// <returns>Массив строк, содержащий уникальные идентификаторы персонажей. В случае ошибки возвращает пустой массив.</returns>
        public static string[] GetAllPersonsOnScene()
        {
            try
            {
                var atoms1 = SuperController.singleton?.GetAtoms();
                if (atoms1 == null)
                    return null;
                var persons1 = new List<string>();
                for (int i1 = 0; i1 < atoms1.Count; i1++)
                {
                    var atom1 = atoms1[i1];
                    if (atom1 != null && atom1.type == "Person" && !atom1.destroyed)
                        persons1.Add(atom1.uid);
                }
                return persons1.ToArray();
            }
            catch (Exception e)
            {
                UIHelper?.OutError($"AtomHelper.GetAllPersonsOnScene() Error: {e.Message}");
            }
            return null;
        }


        /// <summary>
        /// Выполняет безопасный поиск атома на сцене по его уникальному идентификатору (UID).
        /// </summary>
        /// <param name="uid">Уникальный идентификатор атома.</param>
        /// <returns>Экземпляр класса Atom, если объект найден и активен; в противном случае — null.</returns>
        public static Atom FindAtomByUid(
            string uid)
        {
            if (string.IsNullOrEmpty(uid))
                return null;
            try
            {
                var atom1 = SuperController.singleton?.GetAtomByUid(uid);
                if (atom1 != null && !atom1.destroyed)
                    return atom1;
            }
            catch (Exception e)
            {
                UIHelper?.OutError($"AtomHelper.FindAtomByUid() [UID: {uid}] Error: {e.Message}");
            }
            return null;
        }

    }

}