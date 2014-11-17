<?php $this->load->view('common/header'); ?>
<div class="container-fluid">
    <ul class="breadcrumb">
        <li>音楽<?php echo $method ?><span class="divider"></span></li>
        <li class="active">音楽<?php echo $method ?>確認</li>
    </ul>

    <h2>音楽<?php echo $method ?></h2>
    <?php echo validation_errors(); ?>
    <div class="panel panel-info">
        <div class="panel-heading">
            <div class="panel-title">音楽<?php echo $method ?></div>
        </div>
        <?php
        $url = 'music/comp_temp/';
        $url .=!empty($id) ? $id : '';
        echo form_open($url);
        ?>
        <div class="panel-body">
            <table class="table table-bordered">
                <tr>
                    <th>曲名&nbsp;<span class="btn-danger">必須</span></th>
                    <td><?php echo set_value('song_title'); ?></td>
                </tr>
                <tr>
                    <th>作詞&nbsp;<span class="btn-danger">必須</span></th>
                    <td><?php echo set_value('lyricist'); ?></td>
                </tr>
                <tr>
                    <th>作曲&nbsp;<span class="btn-danger">必須</span></th>
                    <td><?php echo set_value('composer'); ?></td>
                </tr>
                <tr>
                    <th>歌手&nbsp;<span class="btn-danger">必須</span></th>
                    <td><?php echo set_value('singer'); ?></td>
                </tr>
                <tr>
                    <th>ジャンル&nbsp;<span class="btn-danger">必須</span></th>
                    <td><?php echo set_value('genre'); ?></td>
                </tr>
                <tr>
                    <th>曲の長さ&nbsp;<span class="btn-danger">必須</span></th>
                    <td><?php echo set_value('song_time'); ?></td>
                </tr>
                <tr>
                    <th>リリース日&nbsp;<span class="btn-danger">必須</span></th>
                    <td>
                        <div class="input-group">
                            <?php echo set_value('release_date'); ?>
                        </div>
                    </td>
                </tr>
            </table>
        </div>
        <?php
        foreach ($post as $key => $val) {
            echo form_hidden($key, $val);
        }
        ?>
        <div class="text-center" style="margin-bottom:10px;"><button type="submit" class="btn btn-success btn-lg">登録</button></div>
        </form>
    </div>
</div>
<?php
$this->load->view('common/footer');
