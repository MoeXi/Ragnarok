/*
 *
 */
#pragma once

// Live2D lib
#include "motion/AMotion.h"
#include "ALive2DModel.h"
#include <string>
#include <vector>

namespace live2d
{
	namespace framework
	{
		/**
		 * Expression�̌v�Z��ނ������܂��B
		 */
		enum ExpressionType
		{
			EXPRESSIONTYPE_SET,
			EXPRESSIONTYPE_ADD,
			EXPRESSIONTYPE_MULTIPLY,
		};

		/**
		 * L2DExpressionBase�œ������p�����[�^��ێ����܂��B
		 */
		struct ExpressionParam
		{
			std::string pid;
            ExpressionType type;
			float value;
		};

		class ExpressionMotion : public AMotion
		{
		private:
            std::vector<ExpressionParam> paramList;

		public:
            virtual ~ExpressionMotion() {}

			/**
			 * �������p�����[�^���X�g���擾���܂��B
			 */
            const std::vector<ExpressionParam> &getParamList() const
			{
				return paramList;
			}

			/**
			 * �������p�����[�^���X�g��ݒ肵�܂��B
			 */
			void setParamList(const std::vector<ExpressionParam> &other)
			{
				paramList = other;
			}

        protected:
			virtual void updateParamExe(live2d::ALive2DModel *model, l2d_int64 timeMSec,
										float weight, MotionQueueEnt *motionQueueEnt);
		};
	}
}