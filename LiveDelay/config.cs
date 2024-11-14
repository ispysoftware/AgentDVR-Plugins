public class configuration {
    
    private bool supportsAudioField;
    
    private bool supportsVideoField;
    
    private int delayField;
    
    public configuration() {
        this.supportsAudioField = true;
        this.supportsVideoField = true;
        this.delayField = 0;
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
    public int Delay {
        get {
            return this.delayField;
        }
        set {
            this.delayField = value;
        }
    }
}
