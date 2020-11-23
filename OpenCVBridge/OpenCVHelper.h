#pragma once

// OpenCVHelper.h
#include <opencv2\core.hpp>
#include <opencv2\highgui.hpp>
#include <opencv2\imgproc.hpp>

namespace OpenCVBridge
{
    public ref class OpenCVHelper sealed
    {
    public:
        /**
        * @fn
        * �R���X�g���N�^
        */
        OpenCVHelper() {}

        /**
        * @fn
        * �Ֆʌ��o����
        * @brief
        * @param (src) ���͉摜
        */
        void DetectFrame(
            Windows::Graphics::Imaging::SoftwareBitmap^ src);

        /**
        * @fn
        * cv::Canny
        * @brief Canny�A���S���Y����p���āC�摜�̃G�b�W�����o���܂�
        * @param (image)
        * @param (edges)
        * @param (threshold1)
        * @param (threshold2)
        * @param (apertureSize)
        * @param (L2gradient)
        */
        void Canny(
            Windows::Graphics::Imaging::SoftwareBitmap^ image,
            Windows::Graphics::Imaging::SoftwareBitmap^ edges,
            double threshold1,
            double threshold2);

    private:
        // helper functions for getting a cv::Mat from SoftwareBitmap
        bool TryConvert(Windows::Graphics::Imaging::SoftwareBitmap^ from, cv::Mat& convertedMat);
        bool GetPointerToPixelData(Windows::Graphics::Imaging::SoftwareBitmap^ bitmap,
            unsigned char** pPixelData, unsigned int* capacity);
    };
}