public class configuration {
    
    private bool supportsAudioField;
    
    private bool supportsVideoField;
    
    private bool volumeEnabledField;
    
    private bool mirrorEnabledField;
    
    private bool alertsEnabledField;
    
    private bool graphicsEnabledField;
    
    private int volumeField;
    
    private int sizeField;
    
    private string example_StringField;
    
    private string example_InfoField;
    
    private bool example_CheckboxField;
    
    private string example_SelectField;
    
    private string example_CommandField;
    
    private string example_SoundField;
    
    private string example_OverlayField;
    
    private int example_SliderField;
    
    private int example_Range_MinField;
    
    private int example_Range_MaxField;
    
    private double example_DecimalField;
    
    private int example_Int32Field;
    
    private string example_ColorField;
    
    private int example_TimeField;
    
    private string example_DateField;
    
    private string example_TextField;
    
    private string example_AreaField;
    
    private string example_ZoneField;
    
    private string example_Trip_WiresField;
    
    public configuration() {
        this.supportsAudioField = true;
        this.supportsVideoField = true;
        this.volumeEnabledField = true;
        this.mirrorEnabledField = true;
        this.alertsEnabledField = true;
        this.graphicsEnabledField = true;
        this.volumeField = 100;
        this.sizeField = 1;
        this.example_StringField = "abc123";
        this.example_InfoField = "Information";
        this.example_CheckboxField = true;
        this.example_SelectField = "steak";
        this.example_CommandField = "";
        this.example_SoundField = "";
        this.example_OverlayField = "";
        this.example_SliderField = 50;
        this.example_Range_MinField = 40;
        this.example_Range_MaxField = 70;
        this.example_DecimalField = 40.5D;
        this.example_Int32Field = 20;
        this.example_ColorField = "#ff0000";
        this.example_TimeField = 1026;
        this.example_DateField = "";
        this.example_TextField = "Any text content";
        this.example_AreaField = "";
        this.example_ZoneField = "111111111111111111111111111111111111111111111111111111111111111111111111111111111" +
            "11111111111111111111111111000000000000000001111111111111111111111111000000000000" +
            "00000000000000000000000000111111111100000000000000000000000000000000000000111111" +
            "11110000000000000000000000000000000000000011111111110000000000111111100000000000" +
            "00000000001111111111111111111111111110000011111111111111111111111111111111111111" +
            "11111000011111111111111111111111111111111111111111111000001111111111111111111111" +
            "11111111111111111111100000111111111111111111111111111111111111111111100000011111" +
            "11111111111111111111111111111111111110000000111111111111111111111111111111111111" +
            "11111100000001111110000001111111111111111111111111111100000000000000000001111111" +
            "11111111111111111111111000000000000000000111111111111111111111111111111100000000" +
            "00000000011111111111111111111111111111111000000000000001111111111111111111111111" +
            "11111110000000000111111111111111111111111111111111111100000000000001111111111111" +
            "11111111111111111111110000000000000011111111111111111111111111111111100000000000" +
            "00000011111111111111111111111111111100000001100000000011111111111111111111111111" +
            "11110000001110000000000111111111111111111111111111110000001110000000000111111111" +
            "11111111111111111111000001111000010000001111111111111111111111111111000001110000" +
            "01000000111111111111111111111111111100000011000001100000011111111111111111111111" +
            "11110000001100000110000001111111111111111111111111110000000100000111000000111111" +
            "11111111111111111111100000000000111100000011111111111111111111111111110000000000" +
            "11111000001111111111111111111111111111000000000011111000000111111111111111111111" +
            "11111110000000001111110000011111111111111111111111111111000000011111110000001111" +
            "11111111111111111111111111111111111111000000111111111111000000000000000000000111" +
            "11111110000011111111111100000000000000000000000000011110000011111111111100000000" +
            "00000000000000000000000111111111111111110000000000000000000000000000000011111111" +
            "11111111111111111111111110000000000000001111111111111111111111111111111111111111" +
            "00000000111111111111110000000000000000000000111111100000111111111111110000000000" +
            "00000000000000001111111111111111111111000000000000000000000000000000111111111111" +
            "11111100000000000000000000000000000000011111111111111111111111111111111100000000" +
            "00000000001111111111111111111111111111111111100000000000001111111111111111111111" +
            "11111111111111110000000000111111111111111111111111111111111111111110000000111111" +
            "11111111111111111111111111111111111111111111111111111111111111111111111111111111" +
            "11111111111111111111111111111111111110000000000111111111111111111111111111111111" +
            "11100000000000001111111111111111111111111111111111000000000000000111111111111111" +
            "11111111111111111100000000000000000111111111111111111111111111111100000001100000" +
            "00001111111111111111111111111111100000011111000000000111111111111111111111111111" +
            "10000001111111000000001111111111111111111111111110000000111111100000000111111111" +
            "11111111111111111000000000000110000000011111111111111111111111111000000000000000" +
            "00000001111111111111111111111111100000000000000000000001111111111111111111111111" +
            "1000000000000000000000011111111";
        this.example_Trip_WiresField = "";
    }
    
    /// <remarks/>
    public bool SupportsAudio {
        get {
            return this.supportsAudioField;
        }
        set {
            this.supportsAudioField = value;
        }
    }
    
    /// <remarks/>
    public bool SupportsVideo {
        get {
            return this.supportsVideoField;
        }
        set {
            this.supportsVideoField = value;
        }
    }
    
    /// <remarks/>
    public bool VolumeEnabled {
        get {
            return this.volumeEnabledField;
        }
        set {
            this.volumeEnabledField = value;
        }
    }
    
    /// <remarks/>
    public bool MirrorEnabled {
        get {
            return this.mirrorEnabledField;
        }
        set {
            this.mirrorEnabledField = value;
        }
    }
    
    /// <remarks/>
    public bool AlertsEnabled {
        get {
            return this.alertsEnabledField;
        }
        set {
            this.alertsEnabledField = value;
        }
    }
    
    /// <remarks/>
    public bool GraphicsEnabled {
        get {
            return this.graphicsEnabledField;
        }
        set {
            this.graphicsEnabledField = value;
        }
    }
    
    /// <remarks/>
    public int Volume {
        get {
            return this.volumeField;
        }
        set {
            this.volumeField = value;
        }
    }
    
    /// <remarks/>
    public int Size {
        get {
            return this.sizeField;
        }
        set {
            this.sizeField = value;
        }
    }
    
    /// <remarks/>
    public string Example_String {
        get {
            return this.example_StringField;
        }
        set {
            this.example_StringField = value;
        }
    }
    
    /// <remarks/>
    public string Example_Info {
        get {
            return this.example_InfoField;
        }
        set {
            this.example_InfoField = value;
        }
    }
    
    /// <remarks/>
    public bool Example_Checkbox {
        get {
            return this.example_CheckboxField;
        }
        set {
            this.example_CheckboxField = value;
        }
    }
    
    /// <remarks/>
    public string Example_Select {
        get {
            return this.example_SelectField;
        }
        set {
            this.example_SelectField = value;
        }
    }
    
    /// <remarks/>
    public string Example_Command {
        get {
            return this.example_CommandField;
        }
        set {
            this.example_CommandField = value;
        }
    }
    
    /// <remarks/>
    public string Example_Sound {
        get {
            return this.example_SoundField;
        }
        set {
            this.example_SoundField = value;
        }
    }
    
    /// <remarks/>
    public string Example_Overlay {
        get {
            return this.example_OverlayField;
        }
        set {
            this.example_OverlayField = value;
        }
    }
    
    /// <remarks/>
    public int Example_Slider {
        get {
            return this.example_SliderField;
        }
        set {
            this.example_SliderField = value;
        }
    }
    
    /// <remarks/>
    public int Example_Range_Min {
        get {
            return this.example_Range_MinField;
        }
        set {
            this.example_Range_MinField = value;
        }
    }
    
    /// <remarks/>
    public int Example_Range_Max {
        get {
            return this.example_Range_MaxField;
        }
        set {
            this.example_Range_MaxField = value;
        }
    }
    
    /// <remarks/>
    public double Example_Decimal {
        get {
            return this.example_DecimalField;
        }
        set {
            this.example_DecimalField = value;
        }
    }
    
    /// <remarks/>
    public int Example_Int32 {
        get {
            return this.example_Int32Field;
        }
        set {
            this.example_Int32Field = value;
        }
    }
    
    /// <remarks/>
    public string Example_Color {
        get {
            return this.example_ColorField;
        }
        set {
            this.example_ColorField = value;
        }
    }
    
    /// <remarks/>
    public int Example_Time {
        get {
            return this.example_TimeField;
        }
        set {
            this.example_TimeField = value;
        }
    }
    
    /// <remarks/>
    public string Example_Date {
        get {
            return this.example_DateField;
        }
        set {
            this.example_DateField = value;
        }
    }
    
    /// <remarks/>
    public string Example_Text {
        get {
            return this.example_TextField;
        }
        set {
            this.example_TextField = value;
        }
    }
    
    /// <remarks/>
    public string Example_Area {
        get {
            return this.example_AreaField;
        }
        set {
            this.example_AreaField = value;
        }
    }
    
    /// <remarks/>
    public string Example_Zone {
        get {
            return this.example_ZoneField;
        }
        set {
            this.example_ZoneField = value;
        }
    }
    
    /// <remarks/>
    public string Example_Trip_Wires {
        get {
            return this.example_Trip_WiresField;
        }
        set {
            this.example_Trip_WiresField = value;
        }
    }
}
