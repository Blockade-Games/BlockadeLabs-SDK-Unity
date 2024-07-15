using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using UnityEditor;
using UnityEngine;

namespace BlockadeLabsSDK.Editor
{
    [CustomPropertyDrawer(typeof(SkyboxStyle))]
    public class SkyboxStylePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            try
            {
                if (!BlockadeLabsSkyboxGenerator.BlockadeLabsClient.HasValidAuthentication)
                {
                    EditorGUI.LabelField(position, "Cannot fetch skybox styles");
                    return;
                }
            }
            catch (AuthenticationException)
            {
                EditorGUI.HelpBox(position, "Check BlockadeLabs api key", MessageType.Error);
                return;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return;
            }

            var id = property.FindPropertyRelative("_id");
            var name = property.FindPropertyRelative("_name");
            var model = property.FindPropertyRelative("_model");

            if (options.Length < 1)
            {
                FetchStyles();

                if (string.IsNullOrWhiteSpace(name.stringValue))
                {
                    EditorGUI.HelpBox(position, "Fetching skybox styles...", MessageType.Info);
                    return;
                }

                EditorGUI.LabelField(position, label, new GUIContent(name.stringValue, id.intValue.ToString()));
                return;
            }

            // dropdown
            var index = -1;
            SkyboxStyle currentOption = null;

            if (id.intValue > 0)
            {
                currentOption = styles?.FirstOrDefault(style => style.Id.ToString() == id.intValue.ToString());
            }

            if (currentOption != null)
            {
                for (var i = 0; i < options.Length; i++)
                {
                    if (options[i].tooltip.Contains(currentOption.Id.ToString()))
                    {
                        index = i;
                        break;
                    }
                }
            }

            EditorGUI.BeginChangeCheck();
            index = EditorGUI.Popup(position, label, index, options);

            if (EditorGUI.EndChangeCheck())
            {
                currentOption = styles?.FirstOrDefault(style => options[index].tooltip.Contains(style.Id.ToString()));
                id.intValue = currentOption!.Id;
                name.stringValue = currentOption!.Name;
                model.intValue = (int)currentOption.Model!;
            }
        }

        private static bool isFetchingStyles;

        public static bool IsFetchingStyles => isFetchingStyles;

        private static List<SkyboxStyle> styles = new List<SkyboxStyle>();

        public static IReadOnlyList<SkyboxStyle> Styles => styles;

        private static GUIContent[] options = Array.Empty<GUIContent>();

        public static IReadOnlyList<GUIContent> Options => options;

        public static async void FetchStyles()
        {
            if (isFetchingStyles) { return; }
            isFetchingStyles = true;

            try
            {
                styles.Clear();
                var result = await BlockadeLabsSkyboxGenerator.BlockadeLabsClient.SkyboxEndpoint.GetSkyboxStyleFamiliesAsync();
                var styleOptions = new List<GUIContent>();

                foreach (var style in result.OrderBy(style => style.SortOrder))
                {
                    var styleName = style.Name.Replace("/", " ");

                    if (style.FamilyStyles != null && style.FamilyStyles.Count > 0)
                    {
                        foreach (var familyStyle in style.FamilyStyles.OrderBy(familyStyle => familyStyle.SortOrder))
                        {
                            styles.Add(familyStyle);
                            var name = familyStyle.Name.Replace("/", " ");
                            styleOptions.Add(new GUIContent($"{familyStyle.Model}/{styleName}/{name}", familyStyle.Id.ToString()));
                        }
                    }
                    else
                    {
                        styles.Add(style);
                        styleOptions.Add(new GUIContent($"{style.Model}/{styleName}", style.Id.ToString()));
                    }
                }

                options = styleOptions.ToArray();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                isFetchingStyles = false;
            }
        }
    }
}
