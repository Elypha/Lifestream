﻿using ECommons.ExcelServices;
using Lifestream.Enums;
using Lifestream.GUI;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Lifestream.Data;
[Serializable]
public class AddressBookEntry
{
    private static JsonSerializerSettings JsonSerializerSettings = new()
    {
        DefaultValueHandling = DefaultValueHandling.Ignore,
    };

		internal Guid GUID = Guid.NewGuid();
    [DefaultValue("")] public string Name = "";
    public int World = 21;
    public ResidentialAetheryteKind City = ResidentialAetheryteKind.Uldah;
    public int Ward = 1;
    public PropertyType PropertyType;
    public int Plot = 1;
    public int Apartment = 1;
    public bool ApartmentSubdivision = false;
    [DefaultValue(false)] public bool AliasEnabled = false;
		[DefaultValue("")] public string Alias = "";

		public bool ShouldSerializeApartment() => PropertyType == PropertyType.Apartment;
		public bool ShouldSerializeApartmentSubdivision() => PropertyType == PropertyType.Apartment;
		public bool ShouldSerializePlot() => PropertyType == PropertyType.House;

    public string GetAddressString()
    {
        if(PropertyType == PropertyType.House)
        {
            return $"{ExcelWorldHelper.GetName(World)}, {TabAddressBook.ResidentialNames.SafeSelect(City)}, W{Ward}, P{Plot}";
        }
				if (PropertyType == PropertyType.Apartment)
				{
						return $"{ExcelWorldHelper.GetName(World)}, {TabAddressBook.ResidentialNames.SafeSelect(City)}, W{Ward}{(ApartmentSubdivision ? " subdivision" : "")}, Apartment {Plot}";
				}
        return "";
		}

    public bool IsValid([NotNullWhen(false)]out string error)
    {
        if (Name == null)
        {
            error = "Name is not a valid string";
						return false;
				}
        if(!ExcelWorldHelper.GetPublicWorlds().Any(x => x.RowId == World))
        {
            error = "World identifier is not valid";
            return false;
        }
        if(!Enum.GetValues<ResidentialAetheryteKind>().Contains(City))
        {
            error = "Residential aetheryte is not valid";
            return false;
        }
        if (Ward < 1 || Ward > 30)
        {
            error = "Ward number is out of range";
            return false;
        }
        if(Plot < 1 || Plot > 60)
        {
            error = "Plot number is out of range";
            return false;
        }
        if(Apartment < 1)
        {
            error = "Apartment number is out of range";
            return false;
        }
				if (Name == null)
				{
						error = "Alias is not a valid string";
						return false;
				}
        error = null;
        return true;
		}
}
