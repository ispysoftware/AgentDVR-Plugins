﻿public partial class configuration {
    
    private int intervalField;
    
    private bool intensiveField;
    
    private bool aZTECField;
    
    private bool cODABARField;
    
    private bool cODE_128Field;
    
    private bool cODE_39Field;
    
    private bool cODE_93Field;
    
    private bool dATA_MATRIXField;
    
    private bool eAN_13Field;
    
    private bool eAN_8Field;
    
    private bool iMBField;
    
    private bool iTFField;
    
    private bool mAXICODEField;
    
    private bool mSIField;
    
    private bool pDF_417Field;
    
    private bool pHARMACODEField;
    
    private bool pLESSEYField;
    
    private bool qR_CODEField;
    
    private bool rSS_14Field;
    
    private bool rSS_ExpandedField;
    
    private bool uPC_AField;
    
    private bool uPC_EField;
    
    private bool uPC_EAN_ExtensionField;
    
    public configuration() {
        this.intervalField = 1000;
        this.intensiveField = true;
        this.aZTECField = false;
        this.cODABARField = false;
        this.cODE_128Field = false;
        this.cODE_39Field = false;
        this.cODE_93Field = false;
        this.dATA_MATRIXField = false;
        this.eAN_13Field = true;
        this.eAN_8Field = true;
        this.iMBField = false;
        this.iTFField = false;
        this.mAXICODEField = false;
        this.mSIField = false;
        this.pDF_417Field = false;
        this.pHARMACODEField = false;
        this.pLESSEYField = false;
        this.qR_CODEField = true;
        this.rSS_14Field = false;
        this.rSS_ExpandedField = false;
        this.uPC_AField = true;
        this.uPC_EField = true;
        this.uPC_EAN_ExtensionField = false;
    }
    
    /// <remarks/>
    public int Interval {
        get {
            return this.intervalField;
        }
        set {
            this.intervalField = value;
        }
    }
    
    /// <remarks/>
    public bool Intensive {
        get {
            return this.intensiveField;
        }
        set {
            this.intensiveField = value;
        }
    }
    
    /// <remarks/>
    public bool AZTEC {
        get {
            return this.aZTECField;
        }
        set {
            this.aZTECField = value;
        }
    }
    
    /// <remarks/>
    public bool CODABAR {
        get {
            return this.cODABARField;
        }
        set {
            this.cODABARField = value;
        }
    }
    
    /// <remarks/>
    public bool CODE_128 {
        get {
            return this.cODE_128Field;
        }
        set {
            this.cODE_128Field = value;
        }
    }
    
    /// <remarks/>
    public bool CODE_39 {
        get {
            return this.cODE_39Field;
        }
        set {
            this.cODE_39Field = value;
        }
    }
    
    /// <remarks/>
    public bool CODE_93 {
        get {
            return this.cODE_93Field;
        }
        set {
            this.cODE_93Field = value;
        }
    }
    
    /// <remarks/>
    public bool DATA_MATRIX {
        get {
            return this.dATA_MATRIXField;
        }
        set {
            this.dATA_MATRIXField = value;
        }
    }
    
    /// <remarks/>
    public bool EAN_13 {
        get {
            return this.eAN_13Field;
        }
        set {
            this.eAN_13Field = value;
        }
    }
    
    /// <remarks/>
    public bool EAN_8 {
        get {
            return this.eAN_8Field;
        }
        set {
            this.eAN_8Field = value;
        }
    }
    
    /// <remarks/>
    public bool IMB {
        get {
            return this.iMBField;
        }
        set {
            this.iMBField = value;
        }
    }
    
    /// <remarks/>
    public bool ITF {
        get {
            return this.iTFField;
        }
        set {
            this.iTFField = value;
        }
    }
    
    /// <remarks/>
    public bool MAXICODE {
        get {
            return this.mAXICODEField;
        }
        set {
            this.mAXICODEField = value;
        }
    }
    
    /// <remarks/>
    public bool MSI {
        get {
            return this.mSIField;
        }
        set {
            this.mSIField = value;
        }
    }
    
    /// <remarks/>
    public bool PDF_417 {
        get {
            return this.pDF_417Field;
        }
        set {
            this.pDF_417Field = value;
        }
    }
    
    /// <remarks/>
    public bool PHARMACODE {
        get {
            return this.pHARMACODEField;
        }
        set {
            this.pHARMACODEField = value;
        }
    }
    
    /// <remarks/>
    public bool PLESSEY {
        get {
            return this.pLESSEYField;
        }
        set {
            this.pLESSEYField = value;
        }
    }
    
    /// <remarks/>
    public bool QR_CODE {
        get {
            return this.qR_CODEField;
        }
        set {
            this.qR_CODEField = value;
        }
    }
    
    /// <remarks/>
    public bool RSS_14 {
        get {
            return this.rSS_14Field;
        }
        set {
            this.rSS_14Field = value;
        }
    }
    
    /// <remarks/>
    public bool RSS_Expanded {
        get {
            return this.rSS_ExpandedField;
        }
        set {
            this.rSS_ExpandedField = value;
        }
    }
    
    /// <remarks/>
    public bool UPC_A {
        get {
            return this.uPC_AField;
        }
        set {
            this.uPC_AField = value;
        }
    }
    
    /// <remarks/>
    public bool UPC_E {
        get {
            return this.uPC_EField;
        }
        set {
            this.uPC_EField = value;
        }
    }
    
    /// <remarks/>
    public bool UPC_EAN_Extension {
        get {
            return this.uPC_EAN_ExtensionField;
        }
        set {
            this.uPC_EAN_ExtensionField = value;
        }
    }
}
