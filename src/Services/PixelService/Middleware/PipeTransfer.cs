using System.Buffers;
using System.IO.Pipelines;

namespace PixelService.Middleware
{
    public static class PipeTransfer
    {
        public static async Task FromPipeReaderToMemoryStreamAsync(PipeReader pipeReader, MemoryStream memoryStream)
        {
            try
            {
                while (true)
                {
                    ReadResult result = await pipeReader.ReadAsync();

                    ReadOnlySequence<byte> buffer = result.Buffer;

                    if (buffer.IsSingleSegment)
                    {
                        memoryStream.Write(buffer.FirstSpan);
                    }
                    else
                    {
                        SequencePosition position = buffer.Start;

                        while (buffer.TryGet(ref position, out ReadOnlyMemory<byte> memory))
                        {
                            memoryStream.Write(memory.Span);
                        }
                    }

                    pipeReader.AdvanceTo(buffer.End);

                    if (result.IsCompleted)
                    {
                        break;
                    }
                }
            }
            finally
            {
                await pipeReader.CompleteAsync();
            }
        }

        public static async Task FromMemoryStreamToPipeWriteAsync(MemoryStream memoryStream, PipeWriter pipeWriter)
        {
            const int minimumBufferSize = 512;

            try
            {
                while (true)
                {
                    Memory<byte> memory = pipeWriter.GetMemory(minimumBufferSize);

                    int bytesRead = await memoryStream.ReadAsync(memory);
                    if (bytesRead == 0)
                    {
                        break;
                    }

                    pipeWriter.Advance(bytesRead);

                    FlushResult result = await pipeWriter.FlushAsync();

                    if (result.IsCompleted)
                    {
                        break;
                    }
                }
            }
            finally
            {
                await pipeWriter.CompleteAsync();
            }
        }
    }
}
