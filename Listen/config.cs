public partial class configuration {
    
    private string listenforField;
    
    private bool enabledField;
    
    private bool alertsField;
    
    private int confidenceField;
    
    public configuration() {
        this.listenforField = "";
        this.enabledField = true;
        this.alertsField = true;
        this.confidenceField = 60;
    }
    
    /// <remarks/>
    public string listenfor {
        get {
            return this.listenforField;
        }
        set {
            this.listenforField = value;
        }
    }
    
    /// <remarks/>
    public bool enabled {
        get {
            return this.enabledField;
        }
        set {
            this.enabledField = value;
        }
    }
    
    /// <remarks/>
    public bool alerts {
        get {
            return this.alertsField;
        }
        set {
            this.alertsField = value;
        }
    }
    
    /// <remarks/>
    public int confidence {
        get {
            return this.confidenceField;
        }
        set {
            this.confidenceField = value;
        }
    }
}
