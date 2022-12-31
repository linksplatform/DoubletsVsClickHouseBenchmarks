using System.Numerics;
using Platform.Converters;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.CriterionMatchers;
using Platform.Data.Doublets.Memory;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Data.Doublets.Sequences.Converters;
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
    public TLinkAddress StartTimeTypeLinkAddress;
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
        StartTimeTypeLinkAddress = CreateType(TypeLinkAddress, nameof(StartTimeTypeLinkAddress));
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
        TLinkAddress startingTimeLinkAddress = UnitedMemoryLinksStorage.GetOrCreate(source: StartTimeTypeLinkAddress, target: AddressToRawNumberConverter.Convert(candle.StartingTime.ToUnixTimeMilliseconds()));
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
                int startingTime = int.Parse(values[0]);
                int openingPrice = int.Parse(values[1]);
                int closingprice = int.Parse(values[2]);
                int highestPrice = int.Parse(values[3]);
                int lowestPrice = int.Parse(values[4]);
                int volume = int.Parse(values[5]);
                TLinkAddress startingTimeLinkAddress = UnitedMemoryLinksStorage.GetOrCreate(source: StartTimeTypeLinkAddress, target: AddressToRawNumberConverter.Convert(startingTime));
                TLinkAddress openingPriceLinkAddress = UnitedMemoryLinksStorage.GetOrCreate(source: OpeningPriceTypeLinkAddress, target: AddressToRawNumberConverter.Convert(openingPrice));
                TLinkAddress closingpriceLinkAddress = UnitedMemoryLinksStorage.GetOrCreate(source: ClosingPriceTypeLinkAddress, target: AddressToRawNumberConverter.Convert(closingprice));
                TLinkAddress highestPriceLinkAddress = UnitedMemoryLinksStorage.GetOrCreate(source: HighestPriceTypeLinkAddress, target: AddressToRawNumberConverter.Convert(highestPrice));
                TLinkAddress lowestPriceLinkAddress = UnitedMemoryLinksStorage.GetOrCreate(source: LowestPriceTypeLinkAddress, target: AddressToRawNumberConverter.Convert(lowestPrice));
                TLinkAddress volumeLinkAddress = UnitedMemoryLinksStorage.GetOrCreate(source: VolumeTypeLinkAddress, target: AddressToRawNumberConverter.Convert(volume));
                List<TLinkAddress> candlePropertyLinkAddressList = new List<TLinkAddress>() { startingTimeLinkAddress, openingPriceLinkAddress, closingpriceLinkAddress, highestPriceLinkAddress, lowestPriceLinkAddress, volumeLinkAddress };
                UnitedMemoryLinksStorage.GetOrCreate(source: CandleTypeLinkAddress, target: BalancedVariantConverter.Convert(candlePropertyLinkAddressList));
            }
        }
    }

    public void GetCandles(DateTimeOffset minimumTime, DateTimeOffset maximumTime)
    {
        List<Candle> candleList = new List<Candle>();
        UnitedMemoryLinksStorage.Each(restriction: new Link<TLinkAddress>(index: UnitedMemoryLinksStorage.Constants.Any, source: StartTimeTypeLinkAddress, target: UnitedMemoryLinksStorage.Constants.Any), handler: link =>
        {
            Link<TLinkAddress> linkStruct = new Link<TLinkAddress>(link);
            TLinkAddress startTime = RawNumberToAddressConverter.Convert(linkStruct.Target);
            if (minimumTime.ToUnixTimeMilliseconds() < startTime && maximumTime.ToUnixTimeMilliseconds() > startTime)
            {
                
            }
            return UnitedMemoryLinksStorage.Constants.Continue;
        });
        return candleList;
    }
}
