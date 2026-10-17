using System;
using UnityEngine;

namespace Meridian
{
    public sealed class PrototypeHud : MonoBehaviour
    {
        private static readonly Color PanelColor = new(0.025f, 0.105f, 0.135f, 0.94f);
        private static readonly Color PanelLightColor = new(0.045f, 0.17f, 0.21f, 0.94f);
        private static readonly Color BorderColor = new(0.28f, 0.58f, 0.63f, 0.55f);
        private static readonly Color AccentColor = new(1f, 0.72f, 0.2f, 1f);
        private static readonly Color TextColor = new(0.91f, 0.97f, 0.98f, 1f);
        private static readonly Color MutedColor = new(0.62f, 0.76f, 0.79f, 1f);

        private ShipController ship;
        private GUIStyle brandStyle;
        private GUIStyle metricLabelStyle;
        private GUIStyle metricValueStyle;
        private GUIStyle controlLabelStyle;
        private GUIStyle controlValueStyle;
        private GUIStyle engineTrackStyle;
        private GUIStyle engineThumbStyle;
        private GUIStyle rudderTrackStyle;
        private GUIStyle rudderThumbStyle;
        private GUIStyle stopButtonStyle;
        private GUIStyle panelStyle;
        private GUIStyle panelLightStyle;
        private Texture2D panelTexture;
        private Texture2D panelLightTexture;
        private Texture2D borderTexture;
        private Texture2D trackTexture;
        private Texture2D accentTexture;
        private Texture2D stopTexture;
        private Texture2D stopPressedTexture;
        private Texture2D forwardZoneTexture;
        private Texture2D reverseZoneTexture;

        public void SetShip(ShipController controller) => ship = controller;

        private void Update()
        {
            if (ship == null) return;

            float step = 0.55f * Time.deltaTime;
            if (Input.GetKey(KeyCode.W))
            {
                ship.SetPortThrottle(ship.PortThrottle + step);
                ship.SetStarboardThrottle(ship.StarboardThrottle + step);
            }
            if (Input.GetKey(KeyCode.S))
            {
                ship.SetPortThrottle(ship.PortThrottle - step);
                ship.SetStarboardThrottle(ship.StarboardThrottle - step);
            }
            if (Input.GetKey(KeyCode.Q)) ship.SetPortThrottle(ship.PortThrottle + step);
            if (Input.GetKey(KeyCode.A)) ship.SetPortThrottle(ship.PortThrottle - step);
            if (Input.GetKey(KeyCode.E)) ship.SetStarboardThrottle(ship.StarboardThrottle + step);
            if (Input.GetKey(KeyCode.D)) ship.SetStarboardThrottle(ship.StarboardThrottle - step);

            float rudderStep = 0.8f * Time.deltaTime;
            if (Input.GetKey(KeyCode.LeftArrow)) ship.SetRudder(ship.Rudder - rudderStep);
            if (Input.GetKey(KeyCode.RightArrow)) ship.SetRudder(ship.Rudder + rudderStep);
            if (Input.GetKeyDown(KeyCode.DownArrow)) ship.SetRudder(0f);
            if (Input.GetKeyDown(KeyCode.Space)) ship.EmergencyStop();
        }

        private void OnGUI()
        {
            if (ship == null) return;

            EnsureStyles();
            UpdateFontSizes();

            float margin = Mathf.Clamp(Screen.width * 0.012f, 8f, 22f);
            float headerHeight = Mathf.Clamp(Screen.height * 0.16f, 58f, 104f);
            DrawTelemetryBar(new Rect(margin, margin, Screen.width - margin * 2f, headerHeight));

            float engineWidth = Mathf.Clamp(Screen.width * 0.135f, 96f, 178f);
            float engineHeight = Mathf.Clamp(Screen.height * 0.52f, 168f, 360f);
            float bottom = Screen.height - margin;
            Rect portArea = new(margin, bottom - engineHeight, engineWidth, engineHeight);
            Rect starboardArea = new(Screen.width - margin - engineWidth, bottom - engineHeight, engineWidth, engineHeight);
            DrawEngineControl(portArea, "İSKELE", ship.PortThrottle, ship.SetPortThrottle);
            DrawEngineControl(starboardArea, "SANCAK", ship.StarboardThrottle, ship.SetStarboardThrottle);

            float availableCenter = Screen.width - (engineWidth + margin) * 2f - margin * 2f;
            float rudderWidth = Mathf.Min(Screen.width * 0.46f, availableCenter);
            float rudderHeight = Mathf.Clamp(Screen.height * 0.25f, 88f, 166f);
            Rect rudderArea = new((Screen.width - rudderWidth) * 0.5f, bottom - rudderHeight, rudderWidth, rudderHeight);
            DrawRudderControl(rudderArea);

            if (Screen.height >= 620f)
            {
                GUI.Label(new Rect((Screen.width - 620f) * 0.5f, margin + headerHeight + 7f, 620f, 24f),
                    "SÜRÜKLE: KAMERA   •   W/S İKİ MAKİNE   •   Q/A İSKELE   •   E/D SANCAK   •   ←/→ DÜMEN",
                    metricLabelStyle);
            }
        }

        private void DrawTelemetryBar(Rect area)
        {
            DrawPanel(area, false);
            float brandWidth = area.width * 0.26f;
            float metricWidth = (area.width - brandWidth) / 4f;

            GUI.Label(new Rect(area.x + 16f, area.y + 8f, brandWidth - 22f, area.height - 16f), "TCG BARBAROS  •  F-244", brandStyle);
            DrawMetric(new Rect(area.x + brandWidth, area.y, metricWidth, area.height), "SÜRAT", $"{ship.SpeedKnots:0.0} kn");
            DrawMetric(new Rect(area.x + brandWidth + metricWidth, area.y, metricWidth, area.height), "BAŞ", $"{ship.Heading:000}°");
            DrawMetric(new Rect(area.x + brandWidth + metricWidth * 2f, area.y, metricWidth, area.height), "ROTA", $"{ship.CourseOverGround:000}°");
            DrawMetric(new Rect(area.x + brandWidth + metricWidth * 3f, area.y, metricWidth, area.height), "MAKİNE", GetEngineState());

            for (int i = 0; i < 4; i++)
            {
                float x = area.x + brandWidth + metricWidth * i;
                GUI.DrawTexture(new Rect(x, area.y + 12f, 1f, area.height - 24f), borderTexture);
            }
        }

        private void DrawMetric(Rect area, string label, string value)
        {
            GUI.Label(new Rect(area.x + 8f, area.y + 7f, area.width - 16f, area.height * 0.36f), label, metricLabelStyle);
            GUI.Label(new Rect(area.x + 8f, area.y + area.height * 0.37f, area.width - 16f, area.height * 0.5f), value, metricValueStyle);
        }

        private void DrawEngineControl(Rect area, string label, float value, Action<float> setter)
        {
            DrawPanel(area, true);
            GUI.Label(new Rect(area.x + 8f, area.y + 7f, area.width - 16f, 24f), label, controlLabelStyle);
            GUI.Label(new Rect(area.x + 8f, area.y + 31f, area.width - 16f, 32f), $"{value * 100f:+0;-0;0}%", controlValueStyle);

            float sliderTop = area.y + 68f;
            float sliderBottom = area.yMax - 18f;
            float sliderHeight = Mathf.Max(70f, sliderBottom - sliderTop);
            GUI.DrawTexture(new Rect(area.center.x - 8f, sliderTop, 16f, sliderHeight * 0.5f), forwardZoneTexture);
            GUI.DrawTexture(new Rect(area.center.x - 8f, sliderTop + sliderHeight * 0.5f, 16f, sliderHeight * 0.5f), reverseZoneTexture);
            float newValue = GUI.VerticalSlider(new Rect(area.center.x - 10f, sliderTop, 20f, sliderHeight), value, 1f, -1f, engineTrackStyle, engineThumbStyle);
            setter(newValue);

            GUI.Label(new Rect(area.x + 5f, sliderTop - 4f, area.width * 0.42f, 20f), "İLERİ", metricLabelStyle);
            GUI.Label(new Rect(area.x + 5f, sliderTop + sliderHeight * 0.5f - 10f, area.width * 0.42f, 20f), "STOP", metricLabelStyle);
            GUI.Label(new Rect(area.x + 5f, sliderBottom - 18f, area.width * 0.44f, 20f), "TORN.", metricLabelStyle);
        }

        private void DrawRudderControl(Rect area)
        {
            DrawPanel(area, true);
            GUI.Label(new Rect(area.x + 12f, area.y + 7f, area.width - 24f, 26f), "DÜMEN", controlLabelStyle);
            GUI.Label(new Rect(area.x + 12f, area.y + 30f, area.width - 24f, 30f), $"{ship.Rudder * 35f:+0;-0;0}°", controlValueStyle);

            float sliderY = area.y + area.height * 0.48f;
            float newRudder = GUI.HorizontalSlider(new Rect(area.x + 26f, sliderY, area.width - 52f, 22f), ship.Rudder, -1f, 1f, rudderTrackStyle, rudderThumbStyle);
            ship.SetRudder(newRudder);

            GUI.Label(new Rect(area.x + 18f, sliderY + 19f, area.width * 0.3f, 20f), "İSKELE", metricLabelStyle);
            GUI.Label(new Rect(area.center.x - area.width * 0.15f, sliderY + 19f, area.width * 0.3f, 20f), "0°", metricLabelStyle);
            GUI.Label(new Rect(area.xMax - area.width * 0.3f - 18f, sliderY + 19f, area.width * 0.3f, 20f), "SANCAK", metricLabelStyle);

            float buttonHeight = Mathf.Clamp(area.height * 0.24f, 25f, 38f);
            Rect stopRect = new(area.center.x - area.width * 0.23f, area.yMax - buttonHeight - 8f, area.width * 0.46f, buttonHeight);
            if (GUI.Button(stopRect, "ACİL STOP", stopButtonStyle)) ship.EmergencyStop();
        }

        private string GetEngineState()
        {
            float average = (ship.PortThrottle + ship.StarboardThrottle) * 0.5f;
            if (Mathf.Abs(average) < 0.03f) return "STOP";
            return average > 0f ? "İLERİ" : "TORNİSTAN";
        }

        private void DrawPanel(Rect area, bool light)
        {
            GUI.Box(area, GUIContent.none, light ? panelLightStyle : panelStyle);
        }

        private void EnsureStyles()
        {
            if (panelTexture != null) return;

            panelTexture = CreateRoundedTexture(64, PanelColor, BorderColor, 13f, 2f);
            panelLightTexture = CreateRoundedTexture(64, PanelLightColor, BorderColor, 13f, 2f);
            borderTexture = CreateTexture(BorderColor);
            trackTexture = CreateRoundedTexture(32, new Color(0.025f, 0.055f, 0.07f, 1f), BorderColor, 9f, 1f);
            accentTexture = CreateCircleTexture(32, AccentColor, new Color(1f, 0.88f, 0.56f, 1f));
            stopTexture = CreateRoundedTexture(48, new Color(0.63f, 0.075f, 0.07f, 1f), new Color(1f, 0.35f, 0.28f, 0.8f), 12f, 2f);
            stopPressedTexture = CreateRoundedTexture(48, new Color(0.88f, 0.16f, 0.12f, 1f), new Color(1f, 0.66f, 0.55f, 0.9f), 12f, 2f);
            forwardZoneTexture = CreateRoundedTexture(24, new Color(0.15f, 0.62f, 0.42f, 0.23f), Color.clear, 8f, 0f);
            reverseZoneTexture = CreateRoundedTexture(24, new Color(0.86f, 0.24f, 0.15f, 0.2f), Color.clear, 8f, 0f);

            panelStyle = new GUIStyle(GUI.skin.box) { border = new RectOffset(15, 15, 15, 15) };
            panelStyle.normal.background = panelTexture;
            panelLightStyle = new GUIStyle(GUI.skin.box) { border = new RectOffset(15, 15, 15, 15) };
            panelLightStyle.normal.background = panelLightTexture;

            brandStyle = NewLabelStyle(TextAnchor.MiddleLeft, TextColor, FontStyle.Bold);
            metricLabelStyle = NewLabelStyle(TextAnchor.MiddleCenter, MutedColor, FontStyle.Normal);
            metricValueStyle = NewLabelStyle(TextAnchor.MiddleCenter, TextColor, FontStyle.Bold);
            controlLabelStyle = NewLabelStyle(TextAnchor.MiddleCenter, TextColor, FontStyle.Bold);
            controlValueStyle = NewLabelStyle(TextAnchor.MiddleCenter, AccentColor, FontStyle.Bold);

            engineTrackStyle = new GUIStyle(GUI.skin.verticalSlider) { fixedWidth = 7f };
            engineTrackStyle.normal.background = trackTexture;
            engineThumbStyle = new GUIStyle(GUI.skin.verticalSliderThumb) { fixedWidth = 24f, fixedHeight = 24f };
            engineThumbStyle.normal.background = accentTexture;
            engineThumbStyle.hover.background = accentTexture;
            engineThumbStyle.active.background = accentTexture;

            rudderTrackStyle = new GUIStyle(GUI.skin.horizontalSlider) { fixedHeight = 7f };
            rudderTrackStyle.normal.background = trackTexture;
            rudderThumbStyle = new GUIStyle(GUI.skin.horizontalSliderThumb) { fixedWidth = 24f, fixedHeight = 24f };
            rudderThumbStyle.normal.background = accentTexture;
            rudderThumbStyle.hover.background = accentTexture;
            rudderThumbStyle.active.background = accentTexture;

            stopButtonStyle = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            stopButtonStyle.border = new RectOffset(13, 13, 13, 13);
            stopButtonStyle.normal.background = stopTexture;
            stopButtonStyle.hover.background = stopPressedTexture;
            stopButtonStyle.active.background = stopPressedTexture;
            stopButtonStyle.normal.textColor = Color.white;
            stopButtonStyle.hover.textColor = Color.white;
            stopButtonStyle.active.textColor = Color.white;
        }

        private void UpdateFontSizes()
        {
            int baseSize = Mathf.Clamp(Mathf.RoundToInt(Screen.height * 0.026f), 11, 22);
            brandStyle.fontSize = Mathf.Clamp(baseSize + 3, 14, 24);
            metricLabelStyle.fontSize = baseSize;
            metricValueStyle.fontSize = Mathf.Clamp(baseSize + 4, 15, 27);
            controlLabelStyle.fontSize = Mathf.Clamp(baseSize + 1, 13, 24);
            controlValueStyle.fontSize = Mathf.Clamp(baseSize + 5, 17, 30);
            stopButtonStyle.fontSize = Mathf.Clamp(baseSize, 12, 21);
        }

        private static GUIStyle NewLabelStyle(TextAnchor alignment, Color color, FontStyle fontStyle)
        {
            return new GUIStyle(GUI.skin.label)
            {
                alignment = alignment,
                fontStyle = fontStyle,
                clipping = TextClipping.Clip,
                normal = { textColor = color }
            };
        }

        private static Texture2D CreateTexture(Color color)
        {
            Texture2D texture = new(1, 1, TextureFormat.RGBA32, false) { hideFlags = HideFlags.HideAndDontSave };
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        private static Texture2D CreateRoundedTexture(int size, Color fill, Color border, float radius, float borderWidth)
        {
            Texture2D texture = new(size, size, TextureFormat.RGBA32, false) { hideFlags = HideFlags.HideAndDontSave };
            Vector2 center = new(size * 0.5f, size * 0.5f);
            Vector2 half = new(size * 0.5f - 1f, size * 0.5f - 1f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 point = new(Mathf.Abs(x - center.x), Mathf.Abs(y - center.y));
                    Vector2 corner = new(Mathf.Max(point.x - (half.x - radius), 0f), Mathf.Max(point.y - (half.y - radius), 0f));
                    float distance = corner.magnitude - radius;
                    if (distance > 0f)
                    {
                        texture.SetPixel(x, y, Color.clear);
                    }
                    else if (borderWidth > 0f && distance > -borderWidth)
                    {
                        texture.SetPixel(x, y, border);
                    }
                    else
                    {
                        texture.SetPixel(x, y, fill);
                    }
                }
            }
            texture.Apply();
            return texture;
        }

        private static Texture2D CreateCircleTexture(int size, Color fill, Color rim)
        {
            Texture2D texture = new(size, size, TextureFormat.RGBA32, false) { hideFlags = HideFlags.HideAndDontSave };
            Vector2 center = new((size - 1f) * 0.5f, (size - 1f) * 0.5f);
            float radius = size * 0.46f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    Color color = distance > radius ? Color.clear : distance > radius - 2f ? rim : fill;
                    texture.SetPixel(x, y, color);
                }
            }
            texture.Apply();
            return texture;
        }
    }
}
