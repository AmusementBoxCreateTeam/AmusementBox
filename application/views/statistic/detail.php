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
            <div id="gct_gender_pie" style="width: 400px;"></div>
            <div id="gct_generation_column" style="width: 1000px;"></div>
        </div>
    </div>
</div>
<?php $this->load->view('common/footer'); ?>

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
</script>