<script type="text/javascript" src="http://www.google.com/jsapi"></script>
<script type="text/javascript">
    google.load("visualization", "1", {packages: ["corechart"]});
    google.setOnLoadCallback(drawChartSamplePie);
    function drawChartSamplePie() {
        var data = google.visualization.arrayToDataTable([
            ['タスク', '人気'],
            ['男', <?php echo $man ?>],
            ['女', <?php echo $woman ?>]
        ]);

        var options = {
            title: '男女比率',
            is3D: true
        };

        var chart = new google.visualization.PieChart(document.getElementById('gct_gender_pie'));
        chart.draw(data, options);
    }

    google.load("visualization", "1", {packages: ["corechart"]});
    google.setOnLoadCallback(
            function() {
                var data = google.visualization.arrayToDataTable([
                    ['', '利用回数'],
                    ['10 代', <?php echo $a10 ?>],
                    ['20 代', <?php echo $a20 ?>],
                    ['30 代', <?php echo $a30 ?>],
                    ['40 代', <?php echo $a40 ?>],
                    ['50 代', <?php echo $a50 ?>],
                    ['60 代', <?php echo $a60 ?>],
                    ['70 代', <?php echo $a70 ?>],
                    ['80 代', <?php echo $a80 ?>],
                    ['90 代', <?php echo $a90 ?>]
                ]);

                var options = {
                    title: '世代別',
                    hAxis: {title: '世代'}
                };

                var chart = new google.visualization.ColumnChart(document.getElementById('gct_generation_column'));
                chart.draw(data, options);
            }
    );
    // Visualization API と折れ線グラフ用のパッケージのロード
    google.load("visualization", "1", {packages: ["corechart"]});
    // Google Visualization API ロード時のコールバック関数の設定
    google.setOnLoadCallback(drawChart);
    // グラフ作成用のコールバック関数
    function drawChart() {
        // データテーブルの作成
        var data = google.visualization.arrayToDataTable([
            ['年度', '利用回数'],
            <?php
            foreach($year_graph as $key => $val){
            ?>
                        [<?php echo "'".$key."'" ?>,<?php echo $val ?>],
    <?php 
            }
    ?>
        ]);
        // グラフのオプションを設定
        var options = {title: '年度別利用回数'};
        // LineChart のオブジェクトの作成
        var chart = new google.visualization.LineChart(document.getElementById('chart_div'));
        // データテーブルとオプションを渡して、グラフを描画 
        chart.draw(data, options);
    }
</script>
<?php $this->load->view('common/header'); ?>
<div class="container-fluid">
    <div class="panel panel-info">
        <div class="panel-heading">
            <div class="panel-title">音楽ランキング詳細</div>
        </div>
        <div class="panel-body">
            <table class="table table-bordered">
                <tr>
                    <th>曲名</th>
                    <td><?php echo $detail->song_title ?></td>
                </tr>
                <tr>
                    <th>作詞</th>
                    <td><?php echo $detail->lyricist ?></td>
                </tr>
                <tr>
                    <th>作曲</th>
                    <td><?php echo $detail->composer ?></td>
                </tr>
                <tr>
                    <th>歌手</th>
                    <td><?php echo $detail->singer ?></td>
                </tr>
                <tr>
                    <th>曲の長さ</th>
                    <td><?php echo $detail->song_time ?></td>
                </tr>
                <tr>
                    <th>ジャンル</th>
                    <td><?php echo $detail->genre ?></td>
                </tr>
                <tr>
                    <th>リリース日</th>
                    <td><?php echo $detail->release_date ?></td>
                </tr>
            </table>
            <table class="table table-bordered">
                <tr>
                    <td>
                        <div id="gct_gender_pie" style="width: 400px;"></div>
                    </td>
                    <td>
                        <div id="gct_generation_column" style="width: 1000px;"></div>
                    </td>
                </tr>
                <tr>
                    <td colspan="2">
                        <div id="chart_div" style="width: 100%; height: 400px;"></div>
                    </td>
                </tr>
            </table>
        </div>
    </div>
        <div class="text-center" style="margin-bottom:10px;">
        <button type="submit" onclick="location.href = '<?php echo base_url() ?>index.php/box/index'" class="btn btn-info btn-lg">
            戻る
        </button>
    </div>
</div>
<?php $this->load->view('common/footer'); ?>