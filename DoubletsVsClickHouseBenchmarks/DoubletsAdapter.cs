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
using Platform.Data.Doublets.Sequences.Numbers.Rational;
using Platform.Data.Doublets.Sequences.Numbers.Raw;
using Platform.Data.Doublets.Sequences.Unicode;
using Platform.Data.Doublets.Sequences.Walkers;
using Platform.Data.Numbers.Raw;
using Platform.IO;
using Platform.Memory;

namespace DoubletsVsClickHouseBenchmarks;

public class DoubletsAdapter<TLinkAddress> : IBenchmarkable where TLinkAddress : struct, IUnsignedNumber<TLinkAddress>, IShiftOperators<TLinkAddress,int,TLinkAddress>, IBitwiseOperators<TLinkAddress,TLinkAddress,TLinkAddress>, IMinMaxValue<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
{
    public AddressToRawNumberConverter<TLinkAddress> AddressToRawNumberConverter;
    public BigIntegerToRawNumberSequenceConverter<TLinkAddress> BigIntegerToRawNumberSequenceConverter;
    public RawNumberSequenceToBigIntegerConverter<TLinkAddress> RawNumberSequenceToBigIntegerConverter;
    public DecimalToRationalConverter<TLinkAddress> DecimalToRationalConverter;
    public RationalToDecimalConverter<TLinkAddress> RationalToDecimalConverter;
    public NumberToLongRawNumberSequenceConverter<long, TLinkAddress> LongNumberToLongRawNumberSequenceConverter;
    public LongRawNumberSequenceToNumberConverter<TLinkAddress, long> LongRawNumberSequenceToLongNumberConverter;

    public BalancedVariantConverter<TLinkAddress> BalancedVariantConverter;
    public LinksConstants<TLinkAddress> LinksConstants;
    public TemporaryFile LinksStorageFilePath = new();
    public RawNumberToAddressConverter<TLinkAddress> RawNumberToAddressConverter;
    public IConverter<string, TLinkAddress> StringToUnicodeSequenceConverter;
    public UnitedMemoryLinks<TLinkAddress> UnitedMemoryLinksStorage;
    public TLinkAddress TypeLinkAddress;
    public TLinkAddress NegativeNumberTypeLinkAddress;
    public TLinkAddress CandleTypeLinkAddress;
    public TLinkAddress StartingTimeTypeLinkAddress;
    public TLinkAddress OpeningPriceTypeLinkAddress;
    public TLinkAddress ClosingPriceTypeLinkAddress;
    public TLinkAddress HighestPriceTypeLinkAddress;
    public TLinkAddress LowestPriceTypeLinkAddress;
    public TLinkAddress VolumeTypeLinkAddress;
    public TLinkAddress CandlePropertiesTypeLinkAddress;

    public DoubletsAdapter()
    {
        Console.WriteLine("New doublets storage");
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
        NegativeNumberTypeLinkAddress = CreateType(TypeLinkAddress, nameof(CandleTypeLinkAddress));
        BigIntegerToRawNumberSequenceConverter = new BigIntegerToRawNumberSequenceConverter<TLinkAddress>(UnitedMemoryLinksStorage, AddressToRawNumberConverter, BalancedVariantConverter, NegativeNumberTypeLinkAddress);
        RawNumberSequenceToBigIntegerConverter = new RawNumberSequenceToBigIntegerConverter<TLinkAddress>(UnitedMemoryLinksStorage, RawNumberToAddressConverter, NegativeNumberTypeLinkAddress);
        LongNumberToLongRawNumberSequenceConverter = new NumberToLongRawNumberSequenceConverter<Int64, TLinkAddress>(UnitedMemoryLinksStorage, AddressToRawNumberConverter);
        LongRawNumberSequenceToLongNumberConverter = new LongRawNumberSequenceToNumberConverter<TLinkAddress, Int64>(UnitedMemoryLinksStorage, RawNumberToAddressConverter);
        RationalToDecimalConverter = new RationalToDecimalConverter<TLinkAddress>(UnitedMemoryLinksStorage, RawNumberSequenceToBigIntegerConverter);
        DecimalToRationalConverter = new DecimalToRationalConverter<TLinkAddress>(UnitedMemoryLinksStorage, BigIntegerToRawNumberSequenceConverter);
        CandleTypeLinkAddress = CreateType(TypeLinkAddress, nameof(CandleTypeLinkAddress));
        StartingTimeTypeLinkAddress = CreateType(TypeLinkAddress, nameof(StartingTimeTypeLinkAddress));
        OpeningPriceTypeLinkAddress = CreateType(TypeLinkAddress, nameof(OpeningPriceTypeLinkAddress));
        ClosingPriceTypeLinkAddress = CreateType(TypeLinkAddress, nameof(ClosingPriceTypeLinkAddress));
        HighestPriceTypeLinkAddress = CreateType(TypeLinkAddress, nameof(HighestPriceTypeLinkAddress));
        LowestPriceTypeLinkAddress = CreateType(TypeLinkAddress, nameof(LowestPriceTypeLinkAddress));
        VolumeTypeLinkAddress = CreateType(TypeLinkAddress, nameof(VolumeTypeLinkAddress));
        CandlePropertiesTypeLinkAddress = CreateType(TypeLinkAddress, nameof(CandlePropertiesTypeLinkAddress));
    }

    public TLinkAddress CreateType(TLinkAddress baseTypeLinkAddress, string name)
    {
        TLinkAddress typeNameStringLinkAddress = StringToUnicodeSequenceConverter.Convert(source: name[..^"TypeLinkAddress".Length]);
        return UnitedMemoryLinksStorage.GetOrCreate(source: baseTypeLinkAddress, target: typeNameStringLinkAddress);
    }

    public void SaveCandle(Candle candle)
    {
        // Console.WriteLine("Saving candle");
        TLinkAddress startingTimeLinkAddress = UnitedMemoryLinksStorage.GetOrCreate(source: StartingTimeTypeLinkAddress, target: LongNumberToLongRawNumberSequenceConverter.Convert(candle.StartingTime.ToUnixTimeSeconds()));
        TLinkAddress openingPriceLinkAddress = UnitedMemoryLinksStorage.GetOrCreate(source: OpeningPriceTypeLinkAddress, target: DecimalToRationalConverter.Convert(candle.OpeningPrice));
        TLinkAddress closingpriceLinkAddress = UnitedMemoryLinksStorage.GetOrCreate(source: ClosingPriceTypeLinkAddress, target: DecimalToRationalConverter.Convert(candle.ClosingPrice));
        TLinkAddress highestPriceLinkAddress = UnitedMemoryLinksStorage.GetOrCreate(source: HighestPriceTypeLinkAddress, target: DecimalToRationalConverter.Convert(candle.HighestPrice));
        TLinkAddress lowestPriceLinkAddress = UnitedMemoryLinksStorage.GetOrCreate(source: LowestPriceTypeLinkAddress, target: DecimalToRationalConverter.Convert(candle.LowestPrice));
        TLinkAddress volumeLinkAddress = UnitedMemoryLinksStorage.GetOrCreate(source: VolumeTypeLinkAddress, target: LongNumberToLongRawNumberSequenceConverter.Convert(candle.Volume));
        List<TLinkAddress> candlePropertyLinkAddressList = new List<TLinkAddress>() { startingTimeLinkAddress, openingPriceLinkAddress, closingpriceLinkAddress, highestPriceLinkAddress, lowestPriceLinkAddress, volumeLinkAddress };
        TLinkAddress candlePropertiesLinkAddress = UnitedMemoryLinksStorage.GetOrCreate(CandlePropertiesTypeLinkAddress, BalancedVariantConverter.Convert(candlePropertyLinkAddressList));
        UnitedMemoryLinksStorage.GetOrCreate(source: CandleTypeLinkAddress, target: candlePropertiesLinkAddress);
    }

    

    public Task SaveCandles(IList<Candle> candles)
    {
        foreach (Candle candle in candles)
        {
            SaveCandle(candle);
        }
        return Task.CompletedTask;
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
        return DateTimeOffset.FromUnixTimeSeconds(LongRawNumberSequenceToLongNumberConverter.Convert(valueLinkAddress));
    }
    
    public decimal GetOpeningPrice(TLinkAddress openingPriceLinkAddress)
    {
        EnsureHasType(openingPriceLinkAddress, OpeningPriceTypeLinkAddress);
        TLinkAddress valueLinkAddress = UnitedMemoryLinksStorage.GetTarget(openingPriceLinkAddress);
        return RationalToDecimalConverter.Convert(valueLinkAddress);
    }
    
    public decimal GetClosingPrice(TLinkAddress closingPriceLinkAddress)
    {
        EnsureHasType(closingPriceLinkAddress, ClosingPriceTypeLinkAddress);
        TLinkAddress valueLinkAddress = UnitedMemoryLinksStorage.GetTarget(closingPriceLinkAddress);
        return RationalToDecimalConverter.Convert(valueLinkAddress);
    }
    
    public decimal GetLowestPrice(TLinkAddress lowestPriceLinkAddress)
    {
        EnsureHasType(lowestPriceLinkAddress, LowestPriceTypeLinkAddress);
        TLinkAddress valueLinkAddress = UnitedMemoryLinksStorage.GetTarget(lowestPriceLinkAddress);
        return RationalToDecimalConverter.Convert(valueLinkAddress);
    }
    
    public decimal GetHighestPrice(TLinkAddress highestPriceLinkAddress)
    {
        EnsureHasType(highestPriceLinkAddress, HighestPriceTypeLinkAddress);
        TLinkAddress valueLinkAddress = UnitedMemoryLinksStorage.GetTarget(highestPriceLinkAddress);
        return RationalToDecimalConverter.Convert(valueLinkAddress);
    }
    
    public long GetVolume(TLinkAddress volumeLinkAddress)
    {
        EnsureHasType(volumeLinkAddress, VolumeTypeLinkAddress);
        TLinkAddress valueLinkAddress = UnitedMemoryLinksStorage.GetTarget(volumeLinkAddress);
        return LongRawNumberSequenceToLongNumberConverter.Convert(valueLinkAddress);
    }

    public IEnumerable<TLinkAddress> GetCandleProperties(TLinkAddress candleLinkAddress)
    {
        TLinkAddress candlePropertiesLinkAddress = UnitedMemoryLinksStorage.GetTarget(candleLinkAddress);
        EnsureHasType(candlePropertiesLinkAddress, CandlePropertiesTypeLinkAddress);
        TLinkAddress candlePropertiesSequenceLinkAddress = UnitedMemoryLinksStorage.GetTarget(candlePropertiesLinkAddress);
        RightSequenceWalker<TLinkAddress> rightSequenceWalker = new RightSequenceWalker<TLinkAddress>(links: UnitedMemoryLinksStorage, new DefaultStack<TLinkAddress>(), isElement: IsCandleProperty);
        return rightSequenceWalker.Walk(candlePropertiesSequenceLinkAddress);
    }

    public Candle GetCandle(TLinkAddress candleLinkAddress)
    {
        EnsureHasType(candleLinkAddress, CandleTypeLinkAddress);
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
            else if (candlePropertyTypeLinkAddress == VolumeTypeLinkAddress)
            {
                candle.Volume = GetVolume(candlePropertyLinkAddress);
            }
            else
            {
                throw new Exception($"The target of {candleLinkAddress} must be a sequence of candle properties");
            }
        }
        return candle;
    }

    public async Task<IList<Candle>> GetCandles(DateTimeOffset minimumTime, DateTimeOffset maximumTime)
    {
        List<Candle> candleList = new List<Candle>();
        UnitedMemoryLinksStorage.Each(restriction: new Link<TLinkAddress>(index: UnitedMemoryLinksStorage.Constants.Any, source: CandleTypeLinkAddress, target: UnitedMemoryLinksStorage.Constants.Any), handler: (IList<TLinkAddress> link) =>
        {
            Candle candle = GetCandle(UnitedMemoryLinksStorage.GetIndex(link));
            if(minimumTime < candle.StartingTime && candle.StartingTime < maximumTime) {
                candleList.Add(candle);
            }
            return UnitedMemoryLinksStorage.Constants.Continue;
        });
        return candleList;
    }

    public async Task RemoveCandles()
    {
        UnitedMemoryLinksStorage.Each(new Link<TLinkAddress>(UnitedMemoryLinksStorage.Constants.Any, CandleTypeLinkAddress, UnitedMemoryLinksStorage.Constants.Any), link =>
        {
            Link<TLinkAddress> linkStruct = new Link<TLinkAddress>(link);
            UnitedMemoryLinksStorage.ClearGarbage(linkStruct.Index);
            return UnitedMemoryLinksStorage.Constants.Continue;
        });
    }
}
