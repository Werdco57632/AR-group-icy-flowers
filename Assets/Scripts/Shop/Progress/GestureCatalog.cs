using System;
using System.Collections.Generic;

namespace IcyFlowers.Shop
{
    public enum HandGesture
    {
        None,
        Point,
        Peace,
        OpenPalm
    }

    public sealed class GestureItem
    {
        public string Id { get; }
        public string Name { get; }
        public string Instruction { get; }
        public string Description { get; }
        public int Price { get; }
        public HandGesture Gesture { get; }

        internal GestureItem(string id, string name, string instruction, string description,
            int price, HandGesture gesture)
        {
            Id = id;
            Name = name;
            Instruction = instruction;
            Description = description;
            Price = price;
            Gesture = gesture;
        }
    }

    public static class GestureCatalog
    {
        public static IReadOnlyList<GestureItem> Items { get; } = Array.AsReadOnly(new[]
        {
            new GestureItem("point", "Pointing", "Raise your index finger.",
                "Keep your other fingers curled to make a pointing pose.", 15, HandGesture.Point),
            new GestureItem("peace", "Peace sign", "Raise your index and middle fingers.",
                "Spread two fingers into a V and curl the others.", 20, HandGesture.Peace),
            new GestureItem("open-palm", "Open palm", "Hold up an open hand.",
                "Extend all five fingers with your palm facing the camera.", 25, HandGesture.OpenPalm)
        });

        public static GestureItem Find(HandGesture gesture)
        {
            foreach (var item in Items)
                if (item.Gesture == gesture) return item;
            return null;
        }
    }
}
