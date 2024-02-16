using System.Collections.Generic;

namespace AzureComputerVision {
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public enum ComputerVisionFeatureType {
        Adult,
        Brands,
        Categories,
        Color,
        Description,
        Faces,
        Objects,
        Tags
    }
    public enum ComputerDetailType {
        Celebrities,
        Landmarks
    }
    public class Root {
        public List<Category> categories { get; set; }
        public List<Tag> tags { get; set; }
        public List<VisionObject> objects { get; set; }
        public Adult adult { get; set; }
        public Description description { get; set; }
        public Metadata metadata { get; set; }
        public List<Face> faces { get; set; }
        public Color color { get; set; }
        public ImageType imageType { get; set; }

        // TODO implement these:
        public string requestId { get; set; }
        public string modelVersion { get; set; }
    }
    public class Adult {
        public bool isAdultContent { get; set; }
        public bool isRacyContent { get; set; }
        public bool isGoryContent { get; set; }
        public double adultScore { get; set; }
        public double racyScore { get; set; }
        public double goreScore { get; set; }
    }

    public class Caption {
        public string text { get; set; }
        public double confidence { get; set; }
    }

    public class Category {
        public string name { get; set; }
        public double score { get; set; }
        public Detail detail { get; set; }
    }

    public class Celebrity {
        public string name { get; set; }
        public FaceRectangle faceRectangle { get; set; }
        public double confidence { get; set; }
    }

    public class Color {
        public string dominantColorForeground { get; set; }
        public string dominantColorBackground { get; set; }
        public List<string> dominantColors { get; set; }
        public string accentColor { get; set; }
        public bool isBWImg { get; set; }
    }

    public class Description {
        public List<string> tags { get; set; }
        public List<Caption> captions { get; set; }
    }

    public class Detail {
        public List<Celebrity> celebrities { get; set; }
        public List<Landmark> landmarks { get; set; }
    }

    public class Face {
        public int age { get; set; }
        public string gender { get; set; }
        public FaceRectangle faceRectangle { get; set; }
    }

    public class FaceRectangle {
        public int left { get; set; }
        public int top { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class ImageType {
        public int clipArtType { get; set; }
        public int lineDrawingType { get; set; }
    }

    public class Landmark {
        public string name { get; set; }
        public double confidence { get; set; }
    }

    public class Metadata {
        public int width { get; set; }
        public int height { get; set; }
        public string format { get; set; }
    }

    public class VisionObject {
        public Rectangle rectangle { get; set; }
        public string @object { get; set; }
        public double confidence { get; set; }
    }

    public class Rectangle {
        public int x { get; set; }
        public int y { get; set; }
        public int w { get; set; }
        public int h { get; set; }
    }

    public class Tag {
        public string name { get; set; }
        public double confidence { get; set; }
    }

    public enum ImageClipArtType {
        NonClipArt = 0,
        Ambiguous,
        NormalClipArt,
        GoodClipArt
    }


}
