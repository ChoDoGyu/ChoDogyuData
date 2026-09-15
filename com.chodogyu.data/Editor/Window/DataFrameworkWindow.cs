using System;
using CDG.Core.Results;
using CDG.Data.Editor.Importing;
using CDG.Data.Validation;
using UnityEditor;
using UnityEngine;

namespace CDG.Data.Editor
{
    /// <summary>
    /// DataTableAsset의 검증, CSV/JSON 가져오기, Preview, Diff 및 Apply 작업을 제공하는
    /// Data Framework 전용 EditorWindow입니다.
    /// </summary>
    internal sealed class DataFrameworkWindow : EditorWindow
    {
        private const string MenuPath = "Tools/ChoDogyu/Data Framework";
        private const string WindowTitle = "CDG Data";

        [SerializeField]
        private UnityEngine.Object targetAsset;

        [SerializeField]
        private DataImportFormat importFormat = DataImportFormat.Csv;

        [SerializeField]
        private string sourceFilePath = string.Empty;

        private IDataImportSession importSession;
        private DataImportSessionPreview currentPreview;

        private string statusMessage = string.Empty;
        private MessageType statusMessageType = MessageType.None;

        private bool validationFoldout = true;
        private bool diffFoldout = true;
        private bool showUnchangedDiffItems = true;

        private Vector2 scrollPosition;

        [MenuItem(MenuPath)]
        private static void Open()
        {
            DataFrameworkWindow window = GetWindow<DataFrameworkWindow>();
            window.ConfigureWindow();

            if (window.targetAsset == null &&
                DataTableAssetTypeUtility.TryGetEntryType(
                    Selection.activeObject,
                    out _))
            {
                window.ChangeTarget(Selection.activeObject);
            }

            window.Show();
        }

        private void OnEnable()
        {
            ConfigureWindow();

            importSession = null;
            currentPreview = null;

            ResetPreviewDisplayState();
            RestoreSession();
        }

        private void OnDisable()
        {
            importSession?.ClearPreview();

            importSession = null;
            currentPreview = null;
        }

        private void OnProjectChange()
        {
            Repaint();
        }

        private void OnGUI()
        {
            EnsureTargetState();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            DrawHeader();

            EditorGUILayout.Space(8f);

            DrawTargetSection();

            EditorGUILayout.Space(8f);

            DrawImportSection();

            if (currentPreview != null)
            {
                EditorGUILayout.Space(8f);
                DrawPreviewSection();

                if (currentPreview != null)
                {
                    EditorGUILayout.Space(8f);
                    DrawValidationSection();

                    if (currentPreview.HasDiff)
                    {
                        EditorGUILayout.Space(8f);
                        DrawDiffSection();
                    }
                }
            }

            if (!string.IsNullOrEmpty(statusMessage))
            {
                EditorGUILayout.Space(8f);

                EditorGUILayout.HelpBox(
                    statusMessage,
                    statusMessageType);
            }

            EditorGUILayout.EndScrollView();
        }

        private void ConfigureWindow()
        {
            titleContent = new GUIContent(WindowTitle);
            minSize = new Vector2(520f, 420f);
        }

        private void DrawHeader()
        {
            EditorGUILayout.LabelField(
                "ChoDogyu Data Framework & Editor",
                EditorStyles.boldLabel);

            EditorGUILayout.LabelField(
                "DataTableAsset을 검증하고 외부 CSV 또는 JSON 데이터를 안전하게 가져옵니다.",
                EditorStyles.wordWrappedLabel);
        }

        private void DrawTargetSection()
        {
            EditorGUILayout.LabelField(
                "Target",
                EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            UnityEngine.Object newTarget = EditorGUILayout.ObjectField(
                "Target Asset",
                targetAsset,
                typeof(ScriptableObject),
                false);

            if (GUILayout.Button(
                "Use Selection",
                GUILayout.Width(100f)))
            {
                UseCurrentSelection();
                newTarget = targetAsset;
            }

            EditorGUILayout.EndHorizontal();

            if (newTarget != targetAsset)
            {
                ChangeTarget(newTarget);
                GUI.FocusControl(null);
            }

            if (targetAsset == null)
            {
                EditorGUILayout.HelpBox(
                    "가져오기 작업을 수행할 DataTableAsset을 선택하세요.",
                    MessageType.Info);

                return;
            }

            if (!DataTableAssetTypeUtility.TryGetEntryType(
                targetAsset,
                out Type entryType))
            {
                EditorGUILayout.HelpBox(
                    "선택한 Asset은 DataTableAsset<T>를 상속하지 않습니다.",
                    MessageType.Error);

                return;
            }

            EditorGUI.BeginDisabledGroup(true);

            EditorGUILayout.TextField(
                "Asset Type",
                targetAsset.GetType().FullName);

            EditorGUILayout.TextField(
                "Entry Type",
                entryType.FullName);

            EditorGUI.EndDisabledGroup();

            if (importSession == null)
            {
                EditorGUILayout.HelpBox(
                    "선택한 Asset에 대한 Import Session을 생성하지 못했습니다.",
                    MessageType.Error);
            }
        }

        private void DrawImportSection()
        {
            EditorGUILayout.LabelField(
                "Import",
                EditorStyles.boldLabel);

            DataImportFormat newFormat =
                (DataImportFormat)EditorGUILayout.EnumPopup(
                    "Format",
                    importFormat);

            if (newFormat != importFormat)
            {
                ChangeImportFormat(newFormat);
            }

            EditorGUI.BeginDisabledGroup(importSession == null);

            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(true);

            EditorGUILayout.TextField(
                "Source File",
                sourceFilePath);

            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button(
                "Browse",
                GUILayout.Width(80f)))
            {
                SelectSourceFile();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4f);

            EditorGUI.BeginDisabledGroup(
                string.IsNullOrEmpty(sourceFilePath));

            if (GUILayout.Button(
                "Create Preview",
                GUILayout.Height(28f)))
            {
                CreatePreview();
            }

            EditorGUI.EndDisabledGroup();
            EditorGUI.EndDisabledGroup();

            if (importSession == null)
            {
                EditorGUILayout.HelpBox(
                    "Preview를 생성하려면 유효한 Target Asset을 먼저 선택하세요.",
                    MessageType.None);

                return;
            }

            if (string.IsNullOrEmpty(sourceFilePath))
            {
                EditorGUILayout.HelpBox(
                    importFormat == DataImportFormat.Csv
                        ? "가져올 CSV 파일을 선택하세요."
                        : "가져올 JSON 파일을 선택하세요.",
                    MessageType.Info);
            }
        }

        private void DrawPreviewSection()
        {
            EditorGUILayout.LabelField(
                "Preview",
                EditorStyles.boldLabel);

            bool canApply = currentPreview.CanApply;

            EditorGUI.BeginDisabledGroup(true);

            EditorGUILayout.IntField(
                "Candidate Count",
                currentPreview.CandidateCount);

            EditorGUILayout.TextField(
                "Validation",
                currentPreview.IsValid
                    ? "Valid"
                    : "Invalid");

            EditorGUILayout.TextField(
                "Diff",
                currentPreview.HasDiff
                    ? "Ready"
                    : "Not Calculated");

            EditorGUILayout.TextField(
                "Apply",
                canApply
                    ? "Ready"
                    : "Unavailable");

            EditorGUI.EndDisabledGroup();

            if (!currentPreview.IsValid)
            {
                EditorGUILayout.HelpBox(
                    $"후보 데이터에서 {currentPreview.ValidationReport.Count}개의 검증 문제가 발견되었습니다.",
                    MessageType.Warning);
            }
            else if (canApply)
            {
                EditorGUILayout.HelpBox(
                    "Validation과 Diff 계산이 완료되었습니다. Apply Preview를 누르면 후보 데이터 전체가 Target Asset을 대체합니다.",
                    MessageType.Info);
            }

            EditorGUILayout.Space(6f);

            EditorGUI.BeginDisabledGroup(!canApply);

            if (GUILayout.Button(
                "Apply Preview",
                GUILayout.Height(30f)))
            {
                ApplyCurrentPreview();
            }

            EditorGUI.EndDisabledGroup();

            if (!canApply)
            {
                EditorGUILayout.HelpBox(
                    "유효한 Validation, Diff 및 상태 Snapshot이 모두 준비되어야 Apply할 수 있습니다.",
                    MessageType.None);
            }
        }

        private void DrawValidationSection()
        {
            validationFoldout = EditorGUILayout.Foldout(
                validationFoldout,
                $"Validation ({currentPreview.ValidationReport.Count})",
                true);

            if (!validationFoldout)
            {
                return;
            }

            if (currentPreview.ValidationReport.IsValid)
            {
                EditorGUILayout.HelpBox(
                    "검증 문제가 없습니다.",
                    MessageType.Info);

                return;
            }

            for (int index = 0; index < currentPreview.ValidationReport.Count; index++)
            {
                DataValidationIssue issue =
                    currentPreview.ValidationReport.Issues[index];

                DrawValidationIssue(
                    index,
                    issue);
            }
        }

        private void DrawValidationIssue(int displayIndex, DataValidationIssue issue)
        {
            EditorGUILayout.BeginVertical(
                EditorStyles.helpBox);

            EditorGUILayout.LabelField(
                $"Issue {displayIndex + 1}",
                EditorStyles.boldLabel);

            EditorGUI.BeginDisabledGroup(true);

            EditorGUILayout.TextField(
                "Type",
                issue.Type.ToString());

            EditorGUILayout.IntField(
                "Entry Index",
                issue.EntryIndex);

            EditorGUILayout.TextField(
                "Entry ID",
                issue.EntryId ?? "(없음)");

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.LabelField(
                "Message",
                EditorStyles.miniBoldLabel);

            EditorGUILayout.LabelField(
                issue.Message,
                EditorStyles.wordWrappedLabel);

            EditorGUILayout.EndVertical();
        }

        private void DrawDiffSection()
        {
            diffFoldout = EditorGUILayout.Foldout(
                diffFoldout,
                $"Diff ({currentPreview.DiffItems.Count})",
                true);

            if (!diffFoldout)
            {
                return;
            }

            DrawDiffSummary();

            EditorGUILayout.Space(4f);

            showUnchangedDiffItems = EditorGUILayout.Toggle(
                "Show Unchanged",
                showUnchangedDiffItems);

            EditorGUILayout.Space(4f);

            int visibleItemCount = 0;

            for (int index = 0; index < currentPreview.DiffItems.Count; index++)
            {
                DataImportSessionDiffItem item =
                    currentPreview.DiffItems[index];

                if (!showUnchangedDiffItems &&
                    item.Type == DataImportDiffType.Unchanged)
                {
                    continue;
                }

                DrawDiffItem(
                    index,
                    item);

                visibleItemCount++;
            }

            if (visibleItemCount == 0)
            {
                EditorGUILayout.HelpBox(
                    showUnchangedDiffItems
                        ? "표시할 Diff 항목이 없습니다."
                        : "변경된 Diff 항목이 없습니다. Unchanged 항목을 보려면 Show Unchanged를 활성화하세요.",
                    MessageType.Info);
            }
        }

        private void DrawDiffSummary()
        {
            EditorGUILayout.BeginVertical(
                EditorStyles.helpBox);

            EditorGUILayout.LabelField(
                "Summary",
                EditorStyles.boldLabel);

            EditorGUI.BeginDisabledGroup(true);

            EditorGUILayout.IntField(
                "Added",
                currentPreview.AddedCount);

            EditorGUILayout.IntField(
                "Removed",
                currentPreview.RemovedCount);

            EditorGUILayout.IntField(
                "Modified",
                currentPreview.ModifiedCount);

            EditorGUILayout.IntField(
                "Unchanged",
                currentPreview.UnchangedCount);

            EditorGUILayout.TextField(
                "Changes",
                currentPreview.HasChanges
                    ? "Yes"
                    : "No");

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndVertical();
        }

        private static void DrawDiffItem(int displayIndex, DataImportSessionDiffItem item)
        {
            EditorGUILayout.BeginHorizontal(
                EditorStyles.helpBox);

            EditorGUILayout.LabelField(
                $"{displayIndex + 1}.",
                GUILayout.Width(28f));

            EditorGUILayout.LabelField(
                item.Type.ToString(),
                GUILayout.Width(85f));

            EditorGUILayout.SelectableLabel(
                item.Id,
                EditorStyles.textField,
                GUILayout.Height(EditorGUIUtility.singleLineHeight));

            EditorGUILayout.EndHorizontal();
        }

        private void UseCurrentSelection()
        {
            UnityEngine.Object selectedObject =
                Selection.activeObject;

            if (selectedObject == null)
            {
                SetStatus(
                    "현재 선택된 Asset이 없습니다.",
                    MessageType.Warning);

                return;
            }

            if (!DataTableAssetTypeUtility.TryGetEntryType(
                selectedObject,
                out _))
            {
                SetStatus(
                    "현재 선택된 Asset은 DataTableAsset<T>가 아닙니다.",
                    MessageType.Error);

                return;
            }

            ChangeTarget(selectedObject);
            GUI.FocusControl(null);
        }

        private void ChangeTarget(UnityEngine.Object newTarget)
        {
            ClearPreview();

            importSession = null;
            targetAsset = newTarget;
            sourceFilePath = string.Empty;

            ResetPreviewDisplayState();
            ClearStatus();

            if (targetAsset == null)
            {
                return;
            }

            if (!DataTableAssetTypeUtility.TryGetEntryType(
                targetAsset,
                out _))
            {
                SetStatus(
                    "선택한 Asset은 DataTableAsset<T>가 아닙니다.",
                    MessageType.Error);

                return;
            }

            Result<IDataImportSession> sessionResult =
                DataImportSessionFactory.Create(targetAsset);

            if (sessionResult.IsFailure)
            {
                SetStatus(
                    sessionResult.Error.Message,
                    MessageType.Error);

                return;
            }

            importSession = sessionResult.Value;

            SetStatus(
                $"Import Session을 생성했습니다. Entry Type: {importSession.EntryType.FullName}",
                MessageType.Info);
        }

        private void ChangeImportFormat(DataImportFormat newFormat)
        {
            importFormat = newFormat;
            sourceFilePath = string.Empty;

            ClearPreview();
            ResetPreviewDisplayState();
            ClearStatus();

            GUI.FocusControl(null);
        }

        private void SelectSourceFile()
        {
            string extension;
            string title;

            switch (importFormat)
            {
                case DataImportFormat.Csv:
                    extension = "csv";
                    title = "Select CSV Data File";
                    break;

                case DataImportFormat.Json:
                    extension = "json";
                    title = "Select JSON Data File";
                    break;

                default:
                    SetStatus(
                        $"지원하지 않는 Import 형식입니다: {importFormat}",
                        MessageType.Error);

                    return;
            }

            string selectedPath = EditorUtility.OpenFilePanel(
                title,
                string.Empty,
                extension);

            if (string.IsNullOrEmpty(selectedPath))
            {
                return;
            }

            sourceFilePath = selectedPath;

            ClearPreview();
            ResetPreviewDisplayState();

            SetStatus(
                $"데이터 파일을 선택했습니다: {sourceFilePath}",
                MessageType.Info);

            GUI.FocusControl(null);
        }

        private void CreatePreview()
        {
            ClearPreview();
            ResetPreviewDisplayState();

            if (importSession == null)
            {
                SetStatus(
                    "Import Session이 없습니다. Target Asset을 다시 선택하세요.",
                    MessageType.Error);

                return;
            }

            Result<string> readResult =
                DataImportSourceFileReader.Read(
                    sourceFilePath,
                    importFormat);

            if (readResult.IsFailure)
            {
                SetStatus(
                    readResult.Error.Message,
                    MessageType.Error);

                return;
            }

            Result<DataImportSessionPreview> previewResult =
                importSession.CreatePreview(
                    readResult.Value,
                    importFormat);

            if (previewResult.IsFailure)
            {
                SetStatus(
                    previewResult.Error.Message,
                    MessageType.Error);

                return;
            }

            currentPreview = previewResult.Value;

            if (!currentPreview.IsValid)
            {
                SetStatus(
                    $"Preview는 생성되었지만 데이터 검증에 실패했습니다. 발견된 문제 수: {currentPreview.ValidationReport.Count}",
                    MessageType.Warning);

                return;
            }

            SetStatus(
                $"Preview 생성 완료. 후보 데이터 수: {currentPreview.CandidateCount}",
                MessageType.Info);
        }

        private void ApplyCurrentPreview()
        {
            if (importSession == null ||
                currentPreview == null)
            {
                SetStatus(
                    "적용할 Preview가 없습니다.",
                    MessageType.Error);

                return;
            }

            bool confirmed = EditorUtility.DisplayDialog(
                "Apply Imported Data",
                $"현재 Target Asset의 데이터를 Import Candidate 전체로 교체합니다.\n\n" +
                $"Candidate: {currentPreview.CandidateCount}\n" +
                $"Added: {currentPreview.AddedCount}\n" +
                $"Removed: {currentPreview.RemovedCount}\n" +
                $"Modified: {currentPreview.ModifiedCount}\n" +
                $"Unchanged: {currentPreview.UnchangedCount}\n\n" +
                "Diff 변경 수가 0이어도 Candidate의 순서와 전체 Snapshot이 적용됩니다.\n" +
                "적용 후에는 Unity Undo로 이전 상태를 복원할 수 있습니다.",
                "Apply",
                "Cancel");

            if (!confirmed)
            {
                SetStatus(
                    "데이터 Apply를 취소했습니다.",
                    MessageType.Info);

                return;
            }

            Result applyResult =
                importSession.ApplyPreview();

            currentPreview = importSession.CurrentPreview;
            ResetPreviewDisplayState();

            if (applyResult.IsFailure)
            {
                SetStatus(
                    $"{applyResult.Error.Message} Preview를 다시 생성하세요.",
                    MessageType.Error);

                Repaint();
                return;
            }

            SetStatus(
                "데이터 Apply가 완료되었습니다. Unity Undo를 사용하여 이전 상태로 되돌릴 수 있습니다.",
                MessageType.Info);

            EditorGUIUtility.PingObject(targetAsset);
            Repaint();
        }

        private void RestoreSession()
        {
            if (targetAsset == null)
            {
                sourceFilePath = string.Empty;
                return;
            }

            if (!DataTableAssetTypeUtility.TryGetEntryType(
                targetAsset,
                out _))
            {
                targetAsset = null;
                sourceFilePath = string.Empty;

                SetStatus(
                    "이전에 선택한 Target Asset을 복원할 수 없습니다.",
                    MessageType.Warning);

                return;
            }

            Result<IDataImportSession> sessionResult =
                DataImportSessionFactory.Create(targetAsset);

            if (sessionResult.IsFailure)
            {
                importSession = null;
                currentPreview = null;

                SetStatus(
                    sessionResult.Error.Message,
                    MessageType.Error);

                return;
            }

            importSession = sessionResult.Value;
            currentPreview = null;

            SetStatus(
                "Editor 상태를 복원했습니다. 이전 Preview는 안전을 위해 폐기되었습니다.",
                MessageType.Info);
        }

        private void EnsureTargetState()
        {
            if (targetAsset != null)
            {
                return;
            }

            if (importSession == null &&
                currentPreview == null &&
                string.IsNullOrEmpty(sourceFilePath))
            {
                return;
            }

            importSession?.ClearPreview();

            importSession = null;
            currentPreview = null;
            sourceFilePath = string.Empty;

            ResetPreviewDisplayState();

            SetStatus(
                "Target Asset 참조가 사라져 Import 상태를 초기화했습니다.",
                MessageType.Warning);
        }

        private void ClearPreview()
        {
            importSession?.ClearPreview();
            currentPreview = null;
        }

        private void ResetPreviewDisplayState()
        {
            validationFoldout = true;
            diffFoldout = true;
            showUnchangedDiffItems = true;
        }

        private void SetStatus(string message, MessageType messageType)
        {
            statusMessage = message ?? string.Empty;
            statusMessageType = messageType;
        }

        private void ClearStatus()
        {
            statusMessage = string.Empty;
            statusMessageType = MessageType.None;
        }
    }
}