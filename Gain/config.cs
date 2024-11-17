public partial class configuration {
    
    private int band1Field;
    
    private int band2Field;
    
    private int band3Field;
    
    private int band4Field;
    
    private int band5Field;
    
    private int band6Field;
    
    private int band7Field;
    
    private bool enabledField;
    
    public configuration() {
        this.band1Field = 0;
        this.band2Field = 0;
        this.band3Field = 0;
        this.band4Field = 0;
        this.band5Field = 0;
        this.band6Field = 0;
        this.band7Field = 0;
        this.enabledField = false;
    }
    
    /// <remarks/>
    public int band1 {
        get {
            return this.band1Field;
        }
        set {
            this.band1Field = value;
        }
    }
    
    /// <remarks/>
    public int band2 {
        get {
            return this.band2Field;
        }
        set {
            this.band2Field = value;
        }
    }
    
    /// <remarks/>
    public int band3 {
        get {
            return this.band3Field;
        }
        set {
            this.band3Field = value;
        }
    }
    
    /// <remarks/>
    public int band4 {
        get {
            return this.band4Field;
        }
        set {
            this.band4Field = value;
        }
    }
    
    /// <remarks/>
    public int band5 {
        get {
            return this.band5Field;
        }
        set {
            this.band5Field = value;
        }
    }
    
    /// <remarks/>
    public int band6 {
        get {
            return this.band6Field;
        }
        set {
            this.band6Field = value;
        }
    }
    
    /// <remarks/>
    public int band7 {
        get {
            return this.band7Field;
        }
        set {
            this.band7Field = value;
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
}
