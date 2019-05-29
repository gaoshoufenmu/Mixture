using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatisticCS
{
    public class Perceptron
    {
        /// <summary>
        /// 第一项为b，对应的x0为1
        /// </summary>
        public double[] w;
        /// <summary>
        /// 学习
        /// </summary>
        /// <param name="data"></param>
        public void Learn(PData data)
        {
            w = new double[data.J];
            var a = 1;                  // 这里简单起见，hardcode步长

            bool flag = true;           // 优化标志位，初始化为true
            while(flag)
            {
                flag = false;       // 标志位复位
                for(int i = 0; i < data.matrix.GetLength(0); i++)
                {
                    var row = data.matrix[i];       // 一行表示一条数据
                    var dist = PUtil.CalcDistance(row, w);
                    while(dist <= 0)
                    {
                        flag = true;        // 表示进行过优化
                        PUtil.Calibrate(w, row, a);
                        dist = PUtil.CalcDistance(row, w);
                    }
                }
            }
        }
        /// <summary>
        /// 对偶形式的学习
        /// </summary>
        /// <param name="data"></param>
        public void Learn_(PData data)
        {
            w = new double[data.J];         // 截距 b 作为 w的第一项
            var N = data.matrix.GetLength(0);       // 数据量
            var gram = PUtil.GetGram(data.matrix);      // 数据点输入的对偶内积矩阵
            var n = new int[N];                     // 每个数据点参与优化的次数
            double a = 1;                       // 步长，hardcode 设置为 1
            bool flag = true;
            while(flag)
            {
                flag = false;
                // 遍历数据点
                for(int i = 0; i < N; i++)
                {
                    var check = PUtil.Check_(data.matrix, i, gram, n, a);
                    while(check < 0)
                    {
                        flag = true;            // 本轮进行了优化
                        n[i] += 1;              // 第 i 个数据点参与优化次数增 1

                        check = PUtil.Check_(data.matrix, i, gram, n, a);
                    }
                }
            }

            // 设置 w 的值
            for(int j = 0; j < data.J; j ++)
            {
                for(int k = 0; k < N; k++)
                {
                    if (j == 0)
                        w[j] += n[k] * a * data.matrix[k][data.J - 1];
                    else
                        w[j] += n[k] * a * data.matrix[k][data.J - 1] * data.matrix[k][j - 1];
                }
            }
        }

        /// <summary>
        /// 根据输入判断输出，输出为-1或1
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public double Judge(double[] input)
        {
            double sum = 0;
            for(int i = 0; i < w.Length; i++)
            {
                if (i == 0)
                    sum += w[i];
                else
                    sum += w[i] * input[i - 1];
            }
            return sum >= 0 ? 1 : -1;
        }
    }

    public class PData
    {
        /// <summary>
        /// 输入数据维度，包括输出，输出在最后一维
        /// </summary>
        public int J;
        /// <summary>
        /// 数据，N*J，其中N表示数据数量
        /// </summary>
        public double[][] matrix;
    }

    public class PUtil
    {
        /// <summary>
        /// 计算点与超平面距离
        /// </summary>
        /// <param name="row">数据点</param>
        /// <param name="w"></param>
        /// <returns></returns>
        public static double CalcDistance(double[] row, double[] w)
        {
            var y = row[row.Length - 1];
            double sum = 0;
            for(int i = 0; i < w.Length; i++)
            {
                if (i == 0)
                    sum += w[0];
                else
                    sum += w[i] * row[i - 1];
            }
            return y * sum;
        }
        /// <summary>
        /// 校正
        /// </summary>
        /// <param name="w">权重</param>
        /// <param name="row">数据点</param>
        /// <param name="a">步长</param>
        public static void Calibrate(double[] w, double[] row, double a)
        {
            var f = row[row.Length - 1] * a;
            for(int i = 0; i < w.Length; i++)
            {
                if (i == 0)
                    w[i] += f;
                else
                    w[i] += row[i - 1];
            }
        }

        /// <summary>
        /// 获取数据输入的对偶内积矩阵
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static double[][] GetGram(double[][] matrix)
        {
            var N = matrix.GetLength(0);
            var J = matrix.GetLength(1);
            var gram = new double[N][];

            for(int i = 0; i < N; i++)
            {
                gram[i] = new double[N];
                for(int k = 0; k < N; k++)
                {
                    double sum = 0;
                    for(int j = 0; j < J; j++)
                    {
                        if (j == 0)
                            sum += 1;
                        else
                            sum += matrix[i][j - 1] * matrix[k][j - 1];
                    }
                    gram[i][k] = sum;
                }
            }
            return gram;
        }
        /// <summary>
        /// 对偶形式的检测条件
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="i"></param>
        /// <param name="gram"></param>
        /// <param name="n"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public static double Check_(double[][] matrix, int i, double[][] gram, int[] n, double a)
        {
            var N = matrix.GetLength(0);    // 样本数据量
            var J = matrix.GetLength(1);   // 数据维度
            var xy = matrix[i];             // 当前检测的数据点
            var multiply = gram[i];         // 关联 i 数据点的 内积向量

            double sigma = 0;
            for(int k = 0; k < N; k++)
            {
                sigma += n[k] * a * matrix[k][J - 1] * multiply[k];
            }
            return matrix[i][J - 1] * sigma;
        }
    }
}
