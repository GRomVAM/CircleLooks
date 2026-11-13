using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GRomVAM
{

    public class UIHelper
        : IDisposable
    {

        private GameObject _msgOverlayGO = null;
        private UnityEngine.UI.Text _msgOverlayText = null;
        private float _msgOverlayTimer = 0f;


        /* consts */


        public readonly Color DEFAULT_COLOR = Color.green;
        public const float DEFAULT_ALPHA = 0.5f;
        public const float DEFAULT_DELAY = 3f;


        /* ctor/dtor */


        public UIHelper(
            string pluginName)
        {
            PluginName = pluginName;
            _makeMessageOverlay();
        }


        public void Dispose()
        {
            _dispose(true);
            GC.SuppressFinalize(this);
        }
        private bool _disposed = false;


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


        public string PluginName { get; }


        /* functions */


        public bool CheckAvailabilityUpdate()
        {
            MuteMessage();
            if ((SuperController.singleton?.isLoading ?? true) || TestActivedInputField())
                return false;
            return true;
        }


        public bool TestActivedInputField()
        {
            var eventSystem1 = UnityEngine.EventSystems.EventSystem.current;
            if (eventSystem1?.currentSelectedGameObject?.GetComponent<UnityEngine.UI.InputField>() != null)
                return true;
            return false;
        }


        public bool TestHotkey(
            JSONStorableStringChooser chooser)
        {
            if (chooser == null)
                return false;
            try
            {
                return chooser.val != "None"
                    && Input.GetKeyDown((KeyCode)Enum.Parse(typeof(KeyCode), chooser.val));
            }
            catch (Exception e)
            {
                OutError($"UIHelper.TestHotkey() [Param: {chooser.name}] Error: {e.Message}");
            }
            return false;
        }


        /* methods */


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


        public void ShowMessage(
            string message,
            Color? textColor = null,
            float? delay = null)
        {
            if (_msgOverlayText == null || _msgOverlayGO == null)
                return;

            var targetCam1 = SuperController.singleton.lookCamera ?? SuperController.singleton.MonitorCenterCamera;
            if (targetCam1 != null && _msgOverlayGO.transform.parent != targetCam1.transform)
                _msgOverlayGO.transform.SetParent(targetCam1.transform, false);

            if (_msgOverlayGO.activeSelf && _msgOverlayTimer > 0f && !string.IsNullOrEmpty(_msgOverlayText.text))
                _msgOverlayText.text += $"\n{message}";
            else
                _msgOverlayText.text = message;

            var color1 = textColor ?? DEFAULT_COLOR;
            color1.a = DEFAULT_ALPHA;
            _msgOverlayText.color = color1;
            _msgOverlayGO.SetActive(true);
            _msgOverlayTimer = delay ?? DEFAULT_DELAY;
        }


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


        private void _makeMessageOverlay()
        {
            _msgOverlayGO = new GameObject("UIHelper_MessageOverlay");
            var targetCam1 = SuperController.singleton.lookCamera ?? SuperController.singleton.MonitorCenterCamera;
            if (targetCam1 != null)
                _msgOverlayGO.transform.SetParent(targetCam1.transform, false);

            _msgOverlayGO.transform.localPosition = new Vector3(0f, 0f, 1.5f);
            var canvas1 = _msgOverlayGO.AddComponent<Canvas>();
            canvas1.renderMode = RenderMode.WorldSpace;
            _msgOverlayGO.AddComponent<UnityEngine.UI.CanvasScaler>().dynamicPixelsPerUnit = 100;

            var canvasRect1 = _msgOverlayGO.GetComponent<RectTransform>();
            canvasRect1.sizeDelta = new Vector2(800f, 300f);
            canvasRect1.localScale = new Vector3(0.004f, 0.004f, 0.004f);

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
            textRect1.sizeDelta = new Vector2(800f, 300f);
            _msgOverlayGO.SetActive(false);
        }


        private void _destroyMessageOverlay()
        {
            if (_msgOverlayGO != null)
                UnityEngine.Object.Destroy(_msgOverlayGO);
        }

    }





    public static class SuppVamIO
    {

        /* consts */


        public const string AppearancePath = "Custom/Atom/Person/Appearance";


        /* properties */


        public static UIHelper UIHelper { get; set; } = null;


        /* functions */


        public static string[] GetFolders(
            string path)
        {
            try
            {
                var a1 = SuperController.singleton?.GetDirectoriesAtPath(path);
                if (a1 == null || a1.Length == 0)
                    return null;
                var items1 = new List<string>();
                for (int i1 = 0; i1 < a1.Length; i1++)
                {
                    var path1 = FixSlashes(a1[i1]);
                    if (!string.IsNullOrEmpty(path1))
                        items1.Add(GetDirname(path1));
                }
                return items1.Count > 0
                    ? items1.ToArray()
                    : null;
            }
            catch (Exception e)
            {
                UIHelper?.OutError($"FileHelper.GetFolders() Error: {e.Message}");
            }
            return null;
        }


        public static string[] GetShortFilenames(
            string path,
            string pattern)
        {
            try
            {
                var a1 = SuperController.singleton?.GetFilesAtPath(path, pattern);
                if (a1 == null || a1.Length == 0)
                    return null;
                var items1 = new List<string>();
                for (int i1 = 0; i1 < a1.Length; i1++)
                {
                    var filename1 = GetShortFilename(a1[i1]);
                    if (!string.IsNullOrEmpty(filename1))
                        items1.Add(filename1);
                }
                return items1.Count > 0
                    ? items1.ToArray()
                    : null;
            }
            catch (Exception e)
            {
                UIHelper?.OutError($"FileHelper.GetShortFilenames() [Path: {path}] Error: {e.Message}");
            }
            return null;
        }


        public static string FixSlashes(
            string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;
            var s1 = path.Replace('\\', '/');
            return s1.EndsWith("/")
                ? s1.Substring(0, s1.Length - 1)
                : s1;
        }


        public static string GetDirname(
            string path)
        {
            int i1 = path.LastIndexOf('/');
            return (i1 != -1)
                ? path.Substring(i1 + 1)
                : path;
        }


        public static string GetShortFilename(
            string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;
            int i1 = path.LastIndexOf('/');
            if (i1 == -1)
                i1 = path.LastIndexOf('\\');
            string s1 = (i1 != -1)
                ? path.Substring(i1 + 1)
                : path;
            int i2 = s1.LastIndexOf('.');
            return (i2 != -1)
                ? s1.Substring(0, i2)
                : s1;
        }

    }





    public static class SuppVamAtom
    {

        private static readonly Dictionary<Atom, Coroutine> _activeBlinks = new Dictionary<Atom, Coroutine>();


        /* properties */


        public static UIHelper UIHelper { get; set; } = null;


        /* functions */


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


        /* methods */


        public static bool ResetAppearancePreset(
            Atom personAtom)
        {
            if (personAtom == null || personAtom.destroyed)
            {
                UIHelper?.OutError("AtomHelper.ResetAppearancePreset() Error: Target atom is null or destroyed.");
                return false;
            }
            if (personAtom.type != "Person")
            {
                UIHelper?.OutError($"AtomHelper.ResetAppearancePreset() Error: Atom '{personAtom.uid}' is not a Person.");
                return false;
            }
            try
            {
                var appearancePresets1 = personAtom.GetStorableByID("AppearancePresets");
                if (appearancePresets1 == null)
                {
                    UIHelper?.OutError($"AtomHelper.ResetAppearancePreset() Error: 'AppearancePresets' storable not found on '{personAtom.uid}'.");
                    return false;
                }

                if (!appearancePresets1.IsAction("ResetToDefaults"))
                {
                    UIHelper?.OutError($"AtomHelper.ResetAppearancePreset() Error: Action 'ResetToDefaults' not found on '{personAtom.uid}'.");
                    return false;
                }

                appearancePresets1.CallAction("ResetToDefaults");
                return true;
            }
            catch (Exception e)
            {
                UIHelper?.OutError($"AtomHelper.ResetAppearancePreset() [UID: {personAtom.uid}] Error: {e.Message}");
            }
            return false;
        }



        public static bool LoadAppearancePreset(
            Atom personAtom,
            string presetPath,
            bool mergeLoad = false)
        {
            if (personAtom == null || personAtom.destroyed)
            {
                UIHelper?.OutError("AtomHelper.LoadAppearancePreset() Error: Target atom is null or destroyed.");
                return false;
            }
            if (personAtom.type != "Person")
            {
                UIHelper?.OutError($"AtomHelper.LoadAppearancePreset() Error: Atom '{personAtom.uid}' is not a Person.");
                return false;
            }
            if (string.IsNullOrEmpty(presetPath))
            {
                UIHelper?.OutError("AtomHelper.LoadAppearancePreset() Error: Preset path is null or empty.");
                return false;
            }
            try
            {
                var appearancePresets1 = personAtom.GetStorableByID("AppearancePresets");
                if (appearancePresets1 == null)
                {
                    UIHelper?.OutError($"AtomHelper.LoadAppearancePreset() Error: 'AppearancePresets' storable not found on '{personAtom.uid}'.");
                    return false;
                }

                string cleanPath1 = presetPath.Replace('\\', '/');
                string actionName1 = mergeLoad
                    ? "MergeLoadPresetWithPath"
                    : "LoadPresetWithPath";

                if (!appearancePresets1.IsPresetFilePathAction(actionName1))
                {
                    UIHelper?.OutError($"AtomHelper.LoadAppearancePreset() Error: Preset file action '{actionName1}' not found on '{personAtom.uid}'.");
                    return false;
                }

                appearancePresets1.CallPresetFileAction(actionName1, cleanPath1);
                return true;
            }
            catch (Exception e)
            {
                UIHelper?.OutError($"AtomHelper.LoadAppearancePreset() [UID: {personAtom.uid}] Error: {e.Message}");
            }
            return false;
        }


        public static void BlinkAtom(
            MonoBehaviour host,
            Atom atom,
            int blinkCount = 2,
            float interval = 0.1f)
        {
            if (host == null || atom == null || atom.destroyed || blinkCount <= 0)
                return;
            try
            {
                Coroutine existingRoutine1;
                if (_activeBlinks.TryGetValue(atom, out existingRoutine1) && existingRoutine1 != null)
                {
                    host.StopCoroutine(existingRoutine1);
                    _activeBlinks.Remove(atom);
                    atom.tempDisableRender = false;
                }
                var routine1 = host.StartCoroutine(_blinkAtomRoutine(atom, blinkCount, interval));
                _activeBlinks[atom] = routine1;
            }
            catch (Exception e)
            {
                UIHelper?.OutError($"AtomHelper.BlinkAtom() [UID: {atom.uid}] Error: {e.Message}");
            }
        }


        public static void StopAllBlinks(
            MonoBehaviour host)
        {
            if (_activeBlinks.Count == 0)
                return;
            try
            {
                foreach (var kvp1 in _activeBlinks)
                {
                    if (host != null && kvp1.Value != null)
                        host.StopCoroutine(kvp1.Value);
                    if (kvp1.Key != null && !kvp1.Key.destroyed)
                        kvp1.Key.tempDisableRender = false;
                }
                _activeBlinks.Clear();
            }
            catch (Exception e)
            {
                UIHelper?.OutError($"AtomHelper.StopAllBlinks() Error: {e.Message}");
            }
        }


        /* privates*/


        private static IEnumerator _blinkAtomRoutine(
            Atom atom,
            int blinkCount,
            float interval)
        {
            bool initialRenderDisabled1 = atom.tempDisableRender;
            var wait1 = new WaitForSecondsRealtime(interval);
            for (int i1 = 0; i1 < blinkCount; i1++)
            {
                if (atom == null || atom.destroyed)
                    yield break;
                atom.tempDisableRender = !initialRenderDisabled1;
                yield return wait1;
                if (atom == null || atom.destroyed)
                    yield break;
                atom.tempDisableRender = initialRenderDisabled1;
                if (i1 < blinkCount - 1)
                    yield return wait1;
            }
            if (atom != null && !atom.destroyed)
                atom.tempDisableRender = initialRenderDisabled1;
            _activeBlinks.Remove(atom);
        }

    }

}