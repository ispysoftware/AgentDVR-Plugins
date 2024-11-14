﻿public partial class configuration {
    
    private string aPIKeyField;
    
    private string uRLField;
    
    private string latLngField;
    
    private int updateFrequencyField;
    
    private int fontSizeField;
    
    private string aPIversionField;
    
    private string foregroundField;
    
    private string backgroundField;
    
    private bool displayBackgroundField;
    
    private int positionField;
    
    private string unitsField;
    
    private int gustLimitField;
    
    private int tempLimitField;
    
    private string statusEventField;
    
    private string displayTypeField;
    
    private string formatField;
    
    public configuration() {
        this.aPIKeyField = "";
        this.uRLField = "";
        this.latLngField = "";
        this.updateFrequencyField = 3600;
        this.fontSizeField = 12;
        this.aPIversionField = "3.0";
        this.foregroundField = "#ffffff";
        this.backgroundField = "#202020";
        this.displayBackgroundField = true;
        this.positionField = 1;
        this.unitsField = "standard";
        this.gustLimitField = 20;
        this.tempLimitField = 40;
        this.statusEventField = "";
        this.displayTypeField = "full";
        this.formatField = "{icon}{main}: {description} \r\n{wind} {windDir} {gust} \r\n{temp} {feelsLike} \r\n{hum" +
            "idity} {uvi}";
    }
    
    /// <remarks/>
    public string APIKey {
        get {
            return this.aPIKeyField;
        }
        set {
            this.aPIKeyField = value;
        }
    }
    
    /// <remarks/>
    public string URL {
        get {
            return this.uRLField;
        }
        set {
            this.uRLField = value;
        }
    }
    
    /// <remarks/>
    public string LatLng {
        get {
            return this.latLngField;
        }
        set {
            this.latLngField = value;
        }
    }
    
    /// <remarks/>
    public int UpdateFrequency {
        get {
            return this.updateFrequencyField;
        }
        set {
            this.updateFrequencyField = value;
        }
    }
    
    /// <remarks/>
    public int FontSize {
        get {
            return this.fontSizeField;
        }
        set {
            this.fontSizeField = value;
        }
    }
    
    /// <remarks/>
    public string APIversion {
        get {
            return this.aPIversionField;
        }
        set {
            this.aPIversionField = value;
        }
    }
    
    /// <remarks/>
    public string Foreground {
        get {
            return this.foregroundField;
        }
        set {
            this.foregroundField = value;
        }
    }
    
    /// <remarks/>
    public string Background {
        get {
            return this.backgroundField;
        }
        set {
            this.backgroundField = value;
        }
    }
    
    /// <remarks/>
    public bool DisplayBackground {
        get {
            return this.displayBackgroundField;
        }
        set {
            this.displayBackgroundField = value;
        }
    }
    
    /// <remarks/>
    public int Position {
        get {
            return this.positionField;
        }
        set {
            this.positionField = value;
        }
    }
    
    /// <remarks/>
    public string Units {
        get {
            return this.unitsField;
        }
        set {
            this.unitsField = value;
        }
    }
    
    /// <remarks/>
    public int GustLimit {
        get {
            return this.gustLimitField;
        }
        set {
            this.gustLimitField = value;
        }
    }
    
    /// <remarks/>
    public int TempLimit {
        get {
            return this.tempLimitField;
        }
        set {
            this.tempLimitField = value;
        }
    }
    
    /// <remarks/>
    public string StatusEvent {
        get {
            return this.statusEventField;
        }
        set {
            this.statusEventField = value;
        }
    }
    
    /// <remarks/>
    public string DisplayType {
        get {
            return this.displayTypeField;
        }
        set {
            this.displayTypeField = value;
        }
    }
    
    /// <remarks/>
    public string Format {
        get {
            return this.formatField;
        }
        set {
            this.formatField = value;
        }
    }
}
