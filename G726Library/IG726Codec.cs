namespace G726Library;

internal interface IG726Codec
{
    int EncodeSample(short inputSample, SampleKind sampleType);
    int DecodeSample(int inputSample, SampleKind sampleType);
}
