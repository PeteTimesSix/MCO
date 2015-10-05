using UnityEngine;
using System.Collections;

public class DashSetup {

    public static DashSetup VIEW = new DashSetup(true, true, false, false, false, false, false);
    public static DashSetup CHOICE = new DashSetup(true, false, true, true, false, false, false);
    public static DashSetup RATE = new DashSetup(true, false, true, false, false, false, true);
    public static DashSetup CUSTOM = new DashSetup(true, true, false, false, false, false, false);
    public static DashSetup ALL = new DashSetup(true, true, true, true, true, true, true);

    public bool enable_dash = true;
    public bool enable_button_any = true;
    public bool enable_button_A = true;
    public bool enable_button_B = true;
    public bool enable_scale_horizontal = true;
    public bool enable_scale_vertical = true;
    public bool enable_scale_rating = true;

    private DashSetup(bool dash, bool button_any, bool button_A, bool button_B, bool scale_h, bool scale_v, bool scale_rating) 
    {
        this.enable_dash = dash;
        this.enable_button_any = button_any;
        this.enable_button_A = button_A;
        this.enable_button_B = button_B;
        this.enable_scale_horizontal = scale_h;
        this.enable_scale_vertical = scale_v;
        this.enable_scale_rating = scale_rating;
    }

    internal bool isActive(DASH_COMPONENT type)
    {
        switch (type) 
        {
            case DASH_COMPONENT.NONE:
                return true;
            case DASH_COMPONENT.DASH:
                return enable_dash;
            case DASH_COMPONENT.BUTTON_ANY:
                return enable_button_any;
            case DASH_COMPONENT.BUTTON_A:
                return enable_button_A;
            case DASH_COMPONENT.BUTTON_B:
                return enable_button_B;
            case DASH_COMPONENT.SCALE_HORIZONTAL:
                return enable_scale_horizontal;
            case DASH_COMPONENT.SCALE_VERTICAL:
                return enable_scale_vertical;
            case DASH_COMPONENT.SCALE_RATING:
                return enable_scale_rating;
        }
        return true;
    }
}
