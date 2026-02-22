#nullable enable

using NUnit.Framework;
using ShaderOp.Customization;
using UnityEngine;

namespace ShaderOp.Tests.Runtime
{
    /// <summary>
    /// CharacterCustomizationDataのテスト
    /// </summary>
    public class CharacterCustomizationDataTests
    {
        [Test]
        public void CharacterCustomizationData_CreateDefault_ReturnsValidData()
        {
            // Act
            CharacterCustomizationData data = CharacterCustomizationData.CreateDefault();

            // Assert
            Assert.IsNotNull(data, "Default data should not be null");
            Assert.AreEqual(Gender.Female, data.Gender, "Default gender should be Female");
            Assert.AreEqual(0, data.SkinToneId, "Default skin tone should be 0");
        }

        [Test]
        public void CharacterCustomizationData_Clone_CreatesDeepCopy()
        {
            // Arrange
            CharacterCustomizationData original = CharacterCustomizationData.CreateDefault();
            original.HairColor = new Color(1.0f, 0.5f, 0.25f);
            original.HairstyleId = 5;

            // Act
            CharacterCustomizationData clone = original.Clone();

            // Assert
            Assert.AreNotSame(original, clone, "Clone should be a different object");
            Assert.AreEqual(original.HairColor, clone.HairColor, "HairColor should match");
            Assert.AreEqual(original.HairstyleId, clone.HairstyleId, "HairstyleId should match");
        }

        [Test]
        public void CharacterCustomizationData_ToJson_ReturnsValidJson()
        {
            // Arrange
            CharacterCustomizationData data = CharacterCustomizationData.CreateDefault();
            data.Gender = Gender.Male;
            data.SkinToneId = 3;

            // Act
            string json = data.ToJson();

            // Assert
            Assert.IsNotEmpty(json, "JSON should not be empty");
            Assert.IsTrue(json.Contains("\"Gender\":1"), "JSON should contain Gender value");
            Assert.IsTrue(json.Contains("\"SkinToneId\":3"), "JSON should contain SkinToneId value");
        }

        [Test]
        public void CharacterCustomizationData_FromJson_DeserializesCorrectly()
        {
            // Arrange
            CharacterCustomizationData original = CharacterCustomizationData.CreateDefault();
            original.Gender = Gender.Male;
            original.SkinToneId = 7;
            original.HairstyleId = 12;
            original.HairColor = new Color(0.8f, 0.4f, 0.2f);
            string json = original.ToJson();

            // Act
            CharacterCustomizationData deserialized = CharacterCustomizationData.FromJson(json);

            // Assert
            Assert.AreEqual(original.Gender, deserialized.Gender, "Gender should match");
            Assert.AreEqual(original.SkinToneId, deserialized.SkinToneId, "SkinToneId should match");
            Assert.AreEqual(original.HairstyleId, deserialized.HairstyleId, "HairstyleId should match");
            Assert.AreEqual(original.HairColor.r, deserialized.HairColor.r, 0.01f, "HairColor.r should match");
            Assert.AreEqual(original.HairColor.g, deserialized.HairColor.g, 0.01f, "HairColor.g should match");
            Assert.AreEqual(original.HairColor.b, deserialized.HairColor.b, 0.01f, "HairColor.b should match");
        }

        [Test]
        public void CharacterCustomizationData_HeightScale_ClampsCorrectly()
        {
            // Arrange
            CharacterCustomizationData data = CharacterCustomizationData.CreateDefault();

            // Act & Assert - Valid range
            data.HeightScale = 0.9f;
            Assert.AreEqual(0.9f, data.HeightScale, 0.01f, "HeightScale should be 0.9");

            // Act & Assert - Below minimum
            data.HeightScale = 0.5f;
            Assert.AreEqual(0.8f, data.HeightScale, 0.01f, "HeightScale should clamp to 0.8");

            // Act & Assert - Above maximum
            data.HeightScale = 1.5f;
            Assert.AreEqual(1.2f, data.HeightScale, 0.01f, "HeightScale should clamp to 1.2");
        }

        [Test]
        public void CharacterCustomizationData_SkinToneId_ClampsCorrectly()
        {
            // Arrange
            CharacterCustomizationData data = CharacterCustomizationData.CreateDefault();

            // Act & Assert - Valid range
            data.SkinToneId = 5;
            Assert.AreEqual(5, data.SkinToneId, "SkinToneId should be 5");

            // Act & Assert - Below minimum
            data.SkinToneId = -1;
            Assert.AreEqual(0, data.SkinToneId, "SkinToneId should clamp to 0");

            // Act & Assert - Above maximum
            data.SkinToneId = 15;
            Assert.AreEqual(9, data.SkinToneId, "SkinToneId should clamp to 9");
        }
    }

    /// <summary>
    /// CharacterCustomizerのテスト
    /// </summary>
    public class CharacterCustomizerTests
    {
        private GameObject _testCharacterObject = null!;
        private CharacterCustomizer _customizer = null!;

        [SetUp]
        public void SetUp()
        {
            // テスト用キャラクターオブジェクトを作成
            _testCharacterObject = new GameObject("TestCharacter");
            _customizer = _testCharacterObject.AddComponent<CharacterCustomizer>();

            // ダミーの体パーツを追加
            GameObject hairObject = new GameObject("Hair");
            hairObject.transform.SetParent(_testCharacterObject.transform);
            MeshRenderer hairRenderer = hairObject.AddComponent<MeshRenderer>();
            hairRenderer.material = new Material(Shader.Find("Standard"));

            GameObject bodyObject = new GameObject("Body");
            bodyObject.transform.SetParent(_testCharacterObject.transform);
            MeshRenderer bodyRenderer = bodyObject.AddComponent<MeshRenderer>();
            bodyRenderer.material = new Material(Shader.Find("Standard"));
        }

        [TearDown]
        public void TearDown()
        {
            if (_testCharacterObject != null)
            {
                Object.DestroyImmediate(_testCharacterObject);
            }
        }

        [Test]
        public void CharacterCustomizer_SetHairColor_UpdatesData()
        {
            // Arrange
            Color testColor = new Color(0.9f, 0.3f, 0.1f);

            // Act
            _customizer.SetHairColor(testColor);
            CharacterCustomizationData data = _customizer.GetCustomizationData();

            // Assert
            Assert.AreEqual(testColor.r, data.HairColor.r, 0.01f, "HairColor.r should match");
            Assert.AreEqual(testColor.g, data.HairColor.g, 0.01f, "HairColor.g should match");
            Assert.AreEqual(testColor.b, data.HairColor.b, 0.01f, "HairColor.b should match");
        }

        [Test]
        public void CharacterCustomizer_SetEyeColor_UpdatesData()
        {
            // Arrange
            Color testColor = new Color(0.1f, 0.5f, 0.9f);

            // Act
            _customizer.SetEyeColor(testColor);
            CharacterCustomizationData data = _customizer.GetCustomizationData();

            // Assert
            Assert.AreEqual(testColor.r, data.EyeColor.r, 0.01f, "EyeColor.r should match");
            Assert.AreEqual(testColor.g, data.EyeColor.g, 0.01f, "EyeColor.g should match");
            Assert.AreEqual(testColor.b, data.EyeColor.b, 0.01f, "EyeColor.b should match");
        }

        [Test]
        public void CharacterCustomizer_SetSkinTone_UpdatesData()
        {
            // Arrange
            int testSkinTone = 7;

            // Act
            _customizer.SetSkinTone(testSkinTone);
            CharacterCustomizationData data = _customizer.GetCustomizationData();

            // Assert
            Assert.AreEqual(testSkinTone, data.SkinToneId, "SkinToneId should match");
        }

        [Test]
        public void CharacterCustomizer_SetHeightScale_UpdatesTransform()
        {
            // Arrange
            float testHeight = 1.1f;

            // Act
            _customizer.SetHeightScale(testHeight);

            // Assert
            Assert.AreEqual(testHeight, _testCharacterObject.transform.localScale.y, 0.01f, "Transform scale.y should match height");
        }

        [Test]
        public void CharacterCustomizer_ResetToDefault_RestoresDefaultValues()
        {
            // Arrange - カスタマイズデータを変更
            _customizer.SetHairColor(new Color(1.0f, 0.0f, 0.0f));
            _customizer.SetSkinTone(5);
            _customizer.SetHeightScale(1.1f);

            // Act
            _customizer.ResetToDefault();
            CharacterCustomizationData data = _customizer.GetCustomizationData();

            // Assert
            CharacterCustomizationData defaultData = CharacterCustomizationData.CreateDefault();
            Assert.AreEqual(defaultData.HairColor.r, data.HairColor.r, 0.01f, "HairColor should be default");
            Assert.AreEqual(defaultData.SkinToneId, data.SkinToneId, "SkinToneId should be default");
            Assert.AreEqual(defaultData.HeightScale, data.HeightScale, 0.01f, "HeightScale should be default");
        }

        [Test]
        public void CharacterCustomizer_GetCustomizationData_ReturnsClone()
        {
            // Arrange
            _customizer.SetHairColor(new Color(0.5f, 0.5f, 0.5f));

            // Act
            CharacterCustomizationData data1 = _customizer.GetCustomizationData();
            CharacterCustomizationData data2 = _customizer.GetCustomizationData();

            // Assert
            Assert.AreNotSame(data1, data2, "Each call should return a new clone");
            Assert.AreEqual(data1.HairColor, data2.HairColor, "But data should be equal");
        }

        [Test]
        public void CharacterCustomizer_OnCustomizationChanged_FiresEvent()
        {
            // Arrange
            bool eventFired = false;
            CharacterCustomizationData? eventData = null;
            _customizer.OnCustomizationChanged += (data) =>
            {
                eventFired = true;
                eventData = data;
            };

            // Act
            _customizer.SetHairColor(new Color(0.7f, 0.3f, 0.1f));

            // Assert
            Assert.IsTrue(eventFired, "Event should have fired");
            Assert.IsNotNull(eventData, "Event data should not be null");
            Assert.AreEqual(0.7f, eventData!.HairColor.r, 0.01f, "Event data should contain updated color");
        }
    }

    /// <summary>
    /// RoomDecorationDataのテスト
    /// </summary>
    public class RoomDecorationDataTests
    {
        [Test]
        public void RoomDecorationData_CreateDefault_ReturnsValidData()
        {
            // Act
            RoomDecorationData data = RoomDecorationData.CreateDefault();

            // Assert
            Assert.IsNotNull(data, "Default data should not be null");
            Assert.AreEqual(0, data.FloorMaterialId, "Default floor material should be 0");
            Assert.AreEqual(0, data.WallMaterialId, "Default wall material should be 0");
            Assert.AreEqual(0, data.PlacedFurniture.Count, "Default should have no furniture");
        }

        [Test]
        public void RoomDecorationData_AddFurniture_AddsToList()
        {
            // Arrange
            RoomDecorationData data = RoomDecorationData.CreateDefault();
            PlacedFurnitureData furniture = new PlacedFurnitureData(1, FurnitureCategory.Furniture, Vector3.zero);

            // Act
            data.AddFurniture(furniture);

            // Assert
            Assert.AreEqual(1, data.PlacedFurniture.Count, "Should have 1 furniture item");
            Assert.AreEqual(furniture, data.PlacedFurniture[0], "Furniture should match");
        }

        [Test]
        public void RoomDecorationData_RemoveFurniture_RemovesFromList()
        {
            // Arrange
            RoomDecorationData data = RoomDecorationData.CreateDefault();
            data.AddFurniture(new PlacedFurnitureData(1, FurnitureCategory.Furniture, Vector3.zero));
            data.AddFurniture(new PlacedFurnitureData(2, FurnitureCategory.Plant, Vector3.one));

            // Act
            data.RemoveFurniture(0);

            // Assert
            Assert.AreEqual(1, data.PlacedFurniture.Count, "Should have 1 furniture item left");
            Assert.AreEqual(2, data.PlacedFurniture[0].FurnitureId, "Remaining furniture should be ID 2");
        }

        [Test]
        public void RoomDecorationData_ClearAllFurniture_RemovesAllItems()
        {
            // Arrange
            RoomDecorationData data = RoomDecorationData.CreateDefault();
            data.AddFurniture(new PlacedFurnitureData(1, FurnitureCategory.Furniture, Vector3.zero));
            data.AddFurniture(new PlacedFurnitureData(2, FurnitureCategory.Plant, Vector3.one));
            data.AddFurniture(new PlacedFurnitureData(3, FurnitureCategory.Lighting, Vector3.up));

            // Act
            data.ClearAllFurniture();

            // Assert
            Assert.AreEqual(0, data.PlacedFurniture.Count, "All furniture should be cleared");
        }

        [Test]
        public void RoomDecorationData_ToJson_ReturnsValidJson()
        {
            // Arrange
            RoomDecorationData data = RoomDecorationData.CreateDefault();
            data.FloorMaterialId = 2;
            data.WallMaterialId = 3;
            data.AddFurniture(new PlacedFurnitureData(5, FurnitureCategory.Wall, new Vector3(1, 2, 3)));

            // Act
            string json = data.ToJson();

            // Assert
            Assert.IsNotEmpty(json, "JSON should not be empty");
            Assert.IsTrue(json.Contains("\"FloorMaterialId\":2"), "JSON should contain FloorMaterialId");
            Assert.IsTrue(json.Contains("\"WallMaterialId\":3"), "JSON should contain WallMaterialId");
        }

        [Test]
        public void RoomDecorationData_FromJson_DeserializesCorrectly()
        {
            // Arrange
            RoomDecorationData original = RoomDecorationData.CreateDefault();
            original.FloorMaterialId = 4;
            original.WallMaterialId = 6;
            original.AmbientColor = new Color(0.8f, 0.9f, 1.0f);
            original.AddFurniture(new PlacedFurnitureData(10, FurnitureCategory.Decoration, new Vector3(5, 6, 7)));
            string json = original.ToJson();

            // Act
            RoomDecorationData deserialized = RoomDecorationData.FromJson(json);

            // Assert
            Assert.AreEqual(original.FloorMaterialId, deserialized.FloorMaterialId, "FloorMaterialId should match");
            Assert.AreEqual(original.WallMaterialId, deserialized.WallMaterialId, "WallMaterialId should match");
            Assert.AreEqual(original.AmbientColor.r, deserialized.AmbientColor.r, 0.01f, "AmbientColor.r should match");
            Assert.AreEqual(original.PlacedFurniture.Count, deserialized.PlacedFurniture.Count, "Furniture count should match");
        }

        [Test]
        public void RoomDecorationData_Clone_CreatesDeepCopy()
        {
            // Arrange
            RoomDecorationData original = RoomDecorationData.CreateDefault();
            original.FloorMaterialId = 5;
            original.AddFurniture(new PlacedFurnitureData(3, FurnitureCategory.Floor, Vector3.zero));

            // Act
            RoomDecorationData clone = original.Clone();

            // Assert
            Assert.AreNotSame(original, clone, "Clone should be a different object");
            Assert.AreEqual(original.FloorMaterialId, clone.FloorMaterialId, "FloorMaterialId should match");
            Assert.AreEqual(original.PlacedFurniture.Count, clone.PlacedFurniture.Count, "Furniture count should match");
            Assert.AreNotSame(original.PlacedFurniture, clone.PlacedFurniture, "Furniture list should be a different object");
        }
    }

    /// <summary>
    /// PlacedFurnitureDataのテスト
    /// </summary>
    public class PlacedFurnitureDataTests
    {
        [Test]
        public void PlacedFurnitureData_Constructor_InitializesCorrectly()
        {
            // Arrange
            int furnitureId = 7;
            FurnitureCategory category = FurnitureCategory.Plant;
            Vector3 position = new Vector3(10, 20, 30);

            // Act
            PlacedFurnitureData data = new PlacedFurnitureData(furnitureId, category, position);

            // Assert
            Assert.AreEqual(furnitureId, data.FurnitureId, "FurnitureId should match");
            Assert.AreEqual(category, data.Category, "Category should match");
            Assert.AreEqual(position, data.Position, "Position should match");
            Assert.AreEqual(Quaternion.identity, data.Rotation, "Rotation should be identity");
            Assert.AreEqual(Vector3.one, data.Scale, "Scale should be one");
            Assert.AreEqual(Color.white, data.Tint, "Tint should be white");
        }

        [Test]
        public void PlacedFurnitureData_SetRotation_UpdatesRotation()
        {
            // Arrange
            PlacedFurnitureData data = new PlacedFurnitureData(1, FurnitureCategory.Furniture, Vector3.zero);
            Quaternion testRotation = Quaternion.Euler(45, 90, 0);

            // Act
            data.Rotation = testRotation;

            // Assert
            Assert.AreEqual(testRotation, data.Rotation, "Rotation should match");
        }

        [Test]
        public void PlacedFurnitureData_SetTint_UpdatesTint()
        {
            // Arrange
            PlacedFurnitureData data = new PlacedFurnitureData(1, FurnitureCategory.Decoration, Vector3.zero);
            Color testTint = new Color(0.5f, 0.7f, 0.9f);

            // Act
            data.Tint = testTint;

            // Assert
            Assert.AreEqual(testTint, data.Tint, "Tint should match");
        }
    }
}
