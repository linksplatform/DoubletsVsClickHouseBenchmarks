using System.Collections;
using System.Numerics;
using Platform.Collections.Stacks;
using Platform.Converters;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.CriterionMatchers;
using Platform.Data.Doublets.Memory;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Data.Doublets.Sequences.Converters;
using Platform.Data.Doublets.Sequences.Walkers;
using Platform.Data.Doublets.Unicode;
using Platform.Data.Numbers.Raw;
using Platform.IO;
using Platform.Memory;

namespace DoubletsVsClickHouseBenchmarks;

public class Doublets<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>
{
    public AddressToRawNumberConverter<TLinkAddress> AddressToRawNumberConverter;
    public BalancedVariantConverter<TLinkAddress> BalancedVariantConverter;
    public LinksConstants<TLinkAddress> LinksConstants;
    public TemporaryFile LinksStorageFilePath = new();
    public RawNumberToAddressConverter<TLinkAddress> RawNumberToAddressConverter;
    public IConverter<string, TLinkAddress> StringToUnicodeSequenceConverter;
    public UnitedMemoryLinks<TLinkAddress> UnitedMemoryLinksStorage;
    public TLinkAddress TypeLinkAddress;
    public TLinkAddress CandleTypeLinkAddress;
    public TLinkAddress StartingTimeTypeLinkAddress;
    public TLinkAddress OpeningPriceTypeLinkAddress;
    public TLinkAddress ClosingPriceTypeLinkAddress;
    public TLinkAddress HighestPriceTypeLinkAddress;
    public TLinkAddress LowestPriceTypeLinkAddress;
    public TLinkAddress VolumeTypeLinkAddress;

    public Doublets()
    {
        LinksConstants = new LinksConstants<TLinkAddress>(enableExternalReferencesSupport: true);
        UnitedMemoryLinksStorage = new UnitedMemoryLinks<TLinkAddress>(memory: new FileMappedResizableDirectMemory(path: LinksStorageFilePath.Filename), memoryReservationStep: UnitedMemoryLinks<TLinkAddress>.DefaultLinksSizeStep, constants: LinksConstants, indexTreeType: IndexTreeType.Default);
        BalancedVariantConverter = new BalancedVariantConverter<TLinkAddress>(links: UnitedMemoryLinksStorage);
        TypeLinkAddress = UnitedMemoryLinksStorage.GetOrCreate(source: TLinkAddress.One, target: TLinkAddress.One);
        var typeIndex = TLinkAddress.One;
        var unicodeSymbolType = UnitedMemoryLinksStorage.GetOrCreate(source: TypeLinkAddress, target: ++typeIndex);
        var unicodeSequenceType = UnitedMemoryLinksStorage.GetOrCreate(source: TypeLinkAddress, target: ++typeIndex);
        BalancedVariantConverter = new BalancedVariantConverter<TLinkAddress>(links: UnitedMemoryLinksStorage);
        TargetMatcher<TLinkAddress> unicodeSymbolCriterionMatcher = new(links: UnitedMemoryLinksStorage, targetToMatch: unicodeSymbolType);
        TargetMatcher<TLinkAddress> unicodeSequenceCriterionMatcher = new(links: UnitedMemoryLinksStorage, targetToMatch: unicodeSequenceType);
        AddressToRawNumberConverter = new AddressToRawNumberConverter<TLinkAddress>();
        RawNumberToAddressConverter = new RawNumberToAddressConverter<TLinkAddress>();
        CharToUnicodeSymbolConverter<TLinkAddress> charToUnicodeSymbolConverter = new(links: UnitedMemoryLinksStorage, addressToNumberConverter: AddressToRawNumberConverter, unicodeSymbolMarker: unicodeSymbolType);
        UnicodeSymbolToCharConverter<TLinkAddress> unicodeSymbolToCharConverter = new(links: UnitedMemoryLinksStorage, numberToAddressConverter: RawNumberToAddressConverter, unicodeSymbolCriterionMatcher: unicodeSymbolCriterionMatcher);
        StringToUnicodeSequenceConverter = new CachingConverterDecorator<string, TLinkAddress>(baseConverter: new StringToUnicodeSequenceConverter<TLinkAddress>(links: UnitedMemoryLinksStorage, charToUnicodeSymbolConverter: charToUnicodeSymbolConverter, listToSequenceLinkConverter: BalancedVariantConverter, unicodeSequenceMarker: unicodeSequenceType));
        CandleTypeLinkAddress = CreateType(TypeLinkAddress, nameof(CandleTypeLinkAddress));
        StartingTimeTypeLinkAddress = CreateType(TypeLinkAddress, nameof(StartingTimeTypeLinkAddress));
        OpeningPriceTypeLinkAddress = CreateType(TypeLinkAddress, nameof(OpeningPriceTypeLinkAddress));
        ClosingPriceTypeLinkAddress = CreateType(TypeLinkAddress, nameof(ClosingPriceTypeLinkAddress));
        HighestPriceTypeLinkAddress = CreateType(TypeLinkAddress, nameof(HighestPriceTypeLinkAddress));
        LowestPriceTypeLinkAddress = CreateType(TypeLinkAddress, nameof(LowestPriceTypeLinkAddress));
        VolumeTypeLinkAddress = CreateType(TypeLinkAddress, nameof(VolumeTypeLinkAddress));
    }

    public TLinkAddress CreateType(TLinkAddress baseTypeLinkAddress, string name)
    {
        TLinkAddress typeNameStringLinkAddress = StringToUnicodeSequenceConverter.Convert(source: name[..^"TypeLinkAddress".Length]);
        return UnitedMemoryLinksStorage.GetOrCreate(source: baseTypeLinkAddress, target: typeNameStringLinkAddress);
    }

    public void SaveCandle(Candle candle)
    {
        TLinkAddress startingTimeLinkAddress = UnitedMemoryLinksStorage.GetOrCreate(source: StartingTimeTypeLinkAddress, target: AddressToRawNumberConverter.Convert(candle.StartingTime.ToUnixTimeMilliseconds()));
        TLinkAddress openingPriceLinkAddress = UnitedMemoryLinksStorage.GetOrCreate(source: OpeningPriceTypeLinkAddress, target: AddressToRawNumberConverter.Convert(candle.OpeningPrice));
        TLinkAddress closingpriceLinkAddress = UnitedMemoryLinksStorage.GetOrCreate(source: ClosingPriceTypeLinkAddress, target: AddressToRawNumberConverter.Convert(candle.ClosingPrice));
        TLinkAddress highestPriceLinkAddress = UnitedMemoryLinksStorage.GetOrCreate(source: HighestPriceTypeLinkAddress, target: AddressToRawNumberConverter.Convert(candle.HighestPrice));
        TLinkAddress lowestPriceLinkAddress = UnitedMemoryLinksStorage.GetOrCreate(source: LowestPriceTypeLinkAddress, target: AddressToRawNumberConverter.Convert(candle.LowestPrice));
        TLinkAddress volumeLinkAddress = UnitedMemoryLinksStorage.GetOrCreate(source: VolumeTypeLinkAddress, target: AddressToRawNumberConverter.Convert(candle.Volume));
        List<TLinkAddress> candlePropertyLinkAddressList = new List<TLinkAddress>() { startingTimeLinkAddress, openingPriceLinkAddress, closingpriceLinkAddress, highestPriceLinkAddress, lowestPriceLinkAddress, volumeLinkAddress };
        UnitedMemoryLinksStorage.GetOrCreate(source: CandleTypeLinkAddress, target: BalancedVariantConverter.Convert(candlePropertyLinkAddressList));
    }

    public Candle ParseCandleFromCsv(string[] cvsValues)
    {
        Candle candle = new Candle()
        {
            StartingTime = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(cvsValues[0])),
            OpeningPrice = decimal.Parse(cvsValues[1]),
            ClosingPrice = decimal.Parse(cvsValues[2]),
            HighestPrice = decimal.Parse(cvsValues[3]),
            LowestPrice = decimal.Parse(cvsValues[4]),
            Volume = long.Parse(cvsValues[5]),
        };
        return candle;
    }

    public void SaveCandles(string cvsFilePath)
    {
        using (var reader = new StreamReader(path: cvsFilePath))
        {
            while (!reader.EndOfStream)
            {
                string? line = reader.ReadLine();
                string[] values = line.Split(separator: ';');
                Candle candle = ParseCandleFromCsv(values);
                SaveCandle(candle);
            }
        }
    }

    public bool IsCandleProperty(TLinkAddress linkAddress)
    {
        TLinkAddress[] allowedTypeLinkAddressArray = new TLinkAddress[]
        {
            StartingTimeTypeLinkAddress, OpeningPriceTypeLinkAddress, ClosingPriceTypeLinkAddress, HighestPriceTypeLinkAddress, LowestPriceTypeLinkAddress, VolumeTypeLinkAddress
        };
        return allowedTypeLinkAddressArray.Contains(UnitedMemoryLinksStorage.GetSource(linkAddress));
    }

    public bool HasType(TLinkAddress linkAddress, TLinkAddress typeLinkAddress)
    {
        return typeLinkAddress == UnitedMemoryLinksStorage.GetSource(linkAddress);
    }

    public void EnsureHasType(TLinkAddress linkAddress, TLinkAddress typeLinkAddress)
    {
        if (!HasType(linkAddress, typeLinkAddress))
        {
            throw new Exception($"{linkAddress} is not of {typeLinkAddress} type. {linkAddress} source must be {typeLinkAddress}");
        }
    }


    public DateTimeOffset GetStartingTime(TLinkAddress startingTimeLinkAddress)
    {
        EnsureHasType(startingTimeLinkAddress, StartingTimeTypeLinkAddress);
        TLinkAddress valueLinkAddress = UnitedMemoryLinksStorage.GetTarget(startingTimeLinkAddress);
        return DateTimeOffset.FromUnixTimeMilliseconds(RawNumberToAddressConverter.Convert(valueLinkAddress));
    }
    
    public long GetOpeningPrice(TLinkAddress openingPriceLinkAddress)
    {
        EnsureHasType(openingPriceLinkAddress, OpeningPriceTypeLinkAddress);
        TLinkAddress valueLinkAddress = UnitedMemoryLinksStorage.GetTarget(openingPriceLinkAddress);
        return RawNumberToAddressConverter.Convert(valueLinkAddress);
    }
    
    public long GetClosingPrice(TLinkAddress closingPriceLinkAddress)
    {
        EnsureHasType(closingPriceLinkAddress, ClosingPriceTypeLinkAddress);
        TLinkAddress valueLinkAddress = UnitedMemoryLinksStorage.GetTarget(closingPriceLinkAddress);
        return RawNumberToAddressConverter.Convert(valueLinkAddress);
    }
    
    public long GetLowestPrice(TLinkAddress lowestPriceLinkAddress)
    {
        EnsureHasType(lowestPriceLinkAddress, LowestPriceTypeLinkAddress);
        TLinkAddress valueLinkAddress = UnitedMemoryLinksStorage.GetTarget(lowestPriceLinkAddress);
        return RawNumberToAddressConverter.Convert(valueLinkAddress);
    }
    
    public long GetHighestPrice(TLinkAddress highestPriceLinkAddress)
    {
        EnsureHasType(highestPriceLinkAddress, HighestPriceTypeLinkAddress);
        TLinkAddress valueLinkAddress = UnitedMemoryLinksStorage.GetTarget(highestPriceLinkAddress);
        return RawNumberToAddressConverter.Convert(valueLinkAddress);
    }
    
    public long GetVolume(TLinkAddress volumeLinkAddress)
    {
        EnsureHasType(volumeLinkAddress, VolumeTypeLinkAddress);
        TLinkAddress valueLinkAddress = UnitedMemoryLinksStorage.GetTarget(volumeLinkAddress);
        return RawNumberToAddressConverter.Convert(valueLinkAddress);
    }

    public IEnumerable<TLinkAddress> GetCandleProperties(TLinkAddress candleLinkAddress)
    {
        TLinkAddress candlePropertiesSequenceLinkAddress = UnitedMemoryLinksStorage.GetTarget(candleLinkAddress);
        RightSequenceWalker<TLinkAddress> rightSequenceWalker = new RightSequenceWalker<TLinkAddress>(links: UnitedMemoryLinksStorage, new DefaultStack<TLinkAddress>(), isElement: IsCandleProperty);
        return rightSequenceWalker.Walk(candlePropertiesSequenceLinkAddress);
    }

    public Candle GetCandle(TLinkAddress candleLinkAddress)
    {
        EnsureHasType(candleLinkAddress, CandleTypeLinkAddress);
        Link<TLinkAddress> linkStruct = new Link<TLinkAddress>(UnitedMemoryLinksStorage.GetLink(candleLinkAddress));
        RightSequenceWalker<TLinkAddress> rightSequenceWalker = new RightSequenceWalker<TLinkAddress>(links: UnitedMemoryLinksStorage, new DefaultStack<TLinkAddress>(), isElement: IsCandleProperty);
        Candle candle = new Candle();
        IEnumerable<TLinkAddress> candleProperties = GetCandleProperties(candleLinkAddress);
        foreach (TLinkAddress candlePropertyLinkAddress in candleProperties)
        {
            TLinkAddress candlePropertyTypeLinkAddress = UnitedMemoryLinksStorage.GetSource(candlePropertyLinkAddress);
            if (candlePropertyTypeLinkAddress == StartingTimeTypeLinkAddress)
            {
                candle.StartingTime = GetStartingTime(candlePropertyLinkAddress);
            } 
            else if (candlePropertyTypeLinkAddress == OpeningPriceTypeLinkAddress)
            {
                candle.OpeningPrice = GetOpeningPrice(candlePropertyLinkAddress);
            }
            else if (candlePropertyTypeLinkAddress == ClosingPriceTypeLinkAddress)
            {
                candle.OpeningPrice = GetClosingPrice(candlePropertyLinkAddress);
            }
            else if (candlePropertyTypeLinkAddress == HighestPriceTypeLinkAddress)
            {
                candle.OpeningPrice = GetHighestPrice(candlePropertyLinkAddress);
            }
            else if (candlePropertyTypeLinkAddress == LowestPriceTypeLinkAddress)
            {
                candle.OpeningPrice = GetLowestPrice(candlePropertyLinkAddress);
            }
            else if (candlePropertyTypeLinkAddress == HighestPriceTypeLinkAddress)
            {
                candle.OpeningPrice = GetHighestPrice(candlePropertyLinkAddress);
            }
            else
            {
                throw new Exception($"The target of {candleLinkAddress} must be a sequence of candle properties");
            }
        }
        return candle;
    }

    public void GetCandles(DateTimeOffset minimumTime, DateTimeOffset maximumTime)
    {
        List<Candle> candleList = new List<Candle>();
        UnitedMemoryLinksStorage.Each(restriction: new Link<TLinkAddress>(index: UnitedMemoryLinksStorage.Constants.Any, source: CandleTypeLinkAddress, target: UnitedMemoryLinksStorage.Constants.Any), handler: (IList<TLinkAddress> link) =>
        {
            Candle candle = GetCandle(UnitedMemoryLinksStorage.GetIndex(link));
            candleList.Add(candle);
            return UnitedMemoryLinksStorage.Constants.Continue;
        });
        return candleList;
    }
}
