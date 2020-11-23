#include "pch.h"
#include "OpenCVHelper.h"
#include "MemoryBuffer.h"

using namespace OpenCVBridge;
using namespace Platform;
using namespace Platform::Collections;
using namespace Windows::Graphics::Imaging;
using namespace Windows::Foundation;
using namespace Windows::Foundation::Collections;
using namespace Microsoft::WRL;
using namespace cv;

bool OpenCVHelper::GetPointerToPixelData(SoftwareBitmap^ bitmap, unsigned char** pPixelData, unsigned int* capacity)
{
    BitmapBuffer^ bmpBuffer = bitmap->LockBuffer(BitmapBufferAccessMode::ReadWrite);
    IMemoryBufferReference^ reference = bmpBuffer->CreateReference();

    ComPtr<IMemoryBufferByteAccess> pBufferByteAccess;
    if ((reinterpret_cast<IInspectable*>(reference)->QueryInterface(IID_PPV_ARGS(&pBufferByteAccess))) != S_OK)
    {
        return false;
    }

    if (pBufferByteAccess->GetBuffer(pPixelData, capacity) != S_OK)
    {
        return false;
    }
    return true;
}

bool OpenCVHelper::TryConvert(SoftwareBitmap^ from, Mat& convertedMat)
{
    unsigned char* pPixels = nullptr;
    unsigned int capacity = 0;
    if (!GetPointerToPixelData(from, &pPixels, &capacity))
    {
        return false;
    }

    Mat mat(from->PixelHeight,
        from->PixelWidth,
        CV_8UC1, // assume input SoftwareBitmap is Gray8
        (void*)pPixels);

    // shallow copy because we want convertedMat.data = pPixels
    // don't use .copyTo or .clone
    convertedMat = mat;
    return true;
}

void OpenCVHelper::DetectFrame(SoftwareBitmap^ src)
{
    // Mat変換
    Mat inputMat, edgesMat;
    if (!TryConvert(src, inputMat))
    {
        return;
    }
    
    // Canny
    cv::Canny(inputMat, edgesMat, 100, 200, 3);

    // 膨張処理
    Mat dilateMat;
    //Mat kernel = cv::getStructuringElement(MORPH_RECT, cv::Size(3, 3));
    dilate(edgesMat, dilateMat, Mat());

    // 輪郭の検出
    std::vector<std::vector<cv::Point>> contours;
    std::vector<cv::Vec4i> hierarchy;
    findContours(dilateMat, contours, hierarchy, RETR_TREE, CHAIN_APPROX_SIMPLE);
     
    std::vector<Mat> rects;
    // 面積でフィルタリング
    for (unsigned int i = 0; i < std::size(contours); i++)
    {
        // 面積が小さいものは除く
        if (contourArea(contours[i]) < 500)
        {
            continue;
        }
        // ルートノードは除く
        if (hierarchy[i][3] == -1)
        {
            continue;
        }
        // 輪郭を囲む長方形を計算する
        RotatedRect rect = minAreaRect(contours[i]);
        Mat rect_points;
        boxPoints(rect, rect_points);
        rects.push_back(rect_points);
    }

    //Vector<Vector<Vector<int>^>^>^ retArray = ref new Vector<Vector<Vector<int>^>^>();
    //for (int i = 0; i < std::size(rects); i++)
    //{
    //    Vector<Vector<int>^>^ oneRects = ref new Vector<Vector<int>^>();
    //    for (int j = 0; j < std::size(rects[0]); j++)
    //    {

    //    }
    //}
    return;
}


void OpenCVHelper::Canny(SoftwareBitmap^ image, SoftwareBitmap^ edges, double threshold1, double threshold2)
{
    Mat inputMat, outputMat;
    if (!(TryConvert(image, inputMat) && TryConvert(edges, outputMat)))
    {
        return;
    }
    cv::Canny(inputMat, outputMat, threshold1, threshold2);
}
