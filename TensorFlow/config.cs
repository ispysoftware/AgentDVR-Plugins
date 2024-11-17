﻿public partial class configuration {
    
    private string modelField;
    
    private string areaField;
    
    private bool overlayField;
    
    private int minConfidenceField;
    
    public configuration() {
        this.modelField = "Inception";
        this.areaField = "";
        this.overlayField = true;
        this.minConfidenceField = 50;
    }
    
    /// <remarks/>
    public string Model {
        get {
            return this.modelField;
        }
        set {
            this.modelField = value;
        }
    }
    
    /// <remarks/>
    public string Area {
        get {
            return this.areaField;
        }
        set {
            this.areaField = value;
        }
    }
    
    /// <remarks/>
    public bool Overlay {
        get {
            return this.overlayField;
        }
        set {
            this.overlayField = value;
        }
    }
    
    /// <remarks/>
    public int MinConfidence {
        get {
            return this.minConfidenceField;
        }
        set {
            this.minConfidenceField = value;
        }
    }
}
