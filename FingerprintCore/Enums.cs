using System;
using System.Collections.Generic;
using System.Text;

namespace FingerprintCore
{
    public enum FingerprintCommand
    {
        GETIMAGE = 0x01,
        IMAGE2TZ = 0x02,
        REGMODEL = 0x05,
        STORE = 0x06,
        LOAD = 0x07,
        UPLOAD = 0x08,
        DELETE = 0x0C,
        EMPTY = 0x0D,
        SETPASSWORD = 0x12,
        VERIFYPASSWORD = 0x13,
        HISPEEDSEARCH = 0x1B,
        TEMPLATECOUNT = 0x1D,
        None = 0xFF
    }

    public enum PacketIdentifier
    {
        COMMANDPACKET = 0x1,
        DATAPACKET = 0x2,
        ACKPACKET = 0x7,
        ENDDATAPACKET = 0x8
    }

    public enum ConfirmationCode
    {
        ExeComplete = 0x00, //< commad execution complete
        ReceiptError = 0x01,    //< error when receiving data package
        NoFingerOnSensor = 0x02,    //< no finger on the sensor
        FailToEnroll = 0x03,    //< fail to enroll the finger
        ImageNotClear = 0x06,   //< fail to generate character file due to the over-disorderly fingerprint image
        ImageToSmall = 0x07,    //< fail to generate character file due to lackness of character point or over-smallness of fingerprint image
        FingerDoesNotMatch = 0x08,  //< finger doesn’t match
        NoMatchFound = 0x09,    //< fail to find the matching finger
        CantCombineCharFiles = 0x0A,    //< fail to combine the character files
        AddressOOB = 0x0B,  //< addressing PageID is beyond the finger library
        InvalidTemplate = 0x0C, //< error when reading template from library or the template is invalid
        TemplateUploadError = 0x0D, //< error when uploading template
        CantReceiveData = 0x0E, //< Module can’t receive the following data packages
        ImageUploadError = 0x0F,    //< error when uploading image
        TemplateDeleteFail = 0x10,  //< fail to delete the template
        LibraryClearFail = 0x11,    //< fail to clear finger library
        PasswordIncorrect = 0x13,   //< wrong password
        NoValidPrimaryImage = 0x15, //< fail to generate the image for the lackness of valid primary image
        FlashWriteError = 0x18, //< error when writing flashe
        NoDefinitionError = 0x19,   //< No definition error
        InvalidRegNr = 0x1A,    //< invalid register number
        RegisterConfigIncorrect = 0x1B, //< incorrect configuration of register
        None = 0xFF //< Not set;
    }

}
