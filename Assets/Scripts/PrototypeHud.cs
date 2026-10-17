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
        private Texture2D panelTexture;
        private Texture2D panelLightTexture;
        private Texture2D borderTexture;
        private Texture2D trackTexture;
        private Texture2D accentTexture;
        private Texture2D stopTexture;
        private Texture2D stopPressedTexture;

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
                    "W/S İKİ MAKİNE   •   Q/A İSKELE   •   E/D SANCAK   •   ←/→ DÜMEN   •   ↓ ORTALA",
                    metricLabelStyle);
            }
        }

        private void DrawTelemetryBar(Rect area)
        {
            DrawPanel(area, false);
            float brandWidth = area.width * 0.22f;
            float metricWidth = (area.width - brandWidth) / 4f;

            GUI.Label(new Rect(area.x + 16f, area.y + 8f, brandWidth - 22f, area.height - 16f), "MERIDIAN", brandStyle);
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
            GUI.DrawTexture(area, light ? panelLightTexture : panelTexture);
            GUI.DrawTexture(new Rect(area.x, area.y, area.width, 1f), borderTexture);
            GUI.DrawTexture(new Rect(area.x, area.yMax - 1f, area.width, 1f), borderTexture);
            GUI.DrawTexture(new Rect(area.x, area.y, 1f, area.height), borderTexture);
            GUI.DrawTexture(new Rect(area.xMax - 1f, area.y, 1f, area.height), borderTexture);
        }

        private void EnsureStyles()
        {
            if (panelTexture != null) return;

            panelTexture = CreateTexture(PanelColor);
            panelLightTexture = CreateTexture(PanelLightColor);
            borderTexture = CreateTexture(BorderColor);
            trackTexture = CreateTexture(new Color(0.04f, 0.08f, 0.10f, 0.95f));
            accentTexture = CreateTexture(AccentColor);
            stopTexture = CreateTexture(new Color(0.68f, 0.12f, 0.11f, 1f));
            stopPressedTexture = CreateTexture(new Color(0.88f, 0.2f, 0.16f, 1f));

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
            brandStyle.fontSize = Mathf.Clamp(baseSize + 7, 17, 30);
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
    }
}
