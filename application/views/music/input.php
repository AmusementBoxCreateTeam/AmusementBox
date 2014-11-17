<?php $this->load->view('common/header'); ?>
<div class="container-fluid">
    <h2>音楽<?php echo $method ?></h2>
    <?php echo validation_errors(); ?>
    <div class="panel panel-info">
        <div class="panel-heading">
            <div class="panel-title">音楽<?php echo $method ?></div>
        </div>
        <?php
        $url = 'music/conf/';
        $url .= !empty($id)?$id:'';
        echo form_open($url); ?>
        <div class="panel-body">
            <table class="table table-bordered">
                <tr>
                    <th>曲名&nbsp;<span class="btn-danger">必須</span></th>
                    <td><input type="text" name="song_title" placeholder="曲名" value="<?php echo set_value('song_title', !empty($data->song_title)?$data->song_title:'') ?>"/></td>
                </tr>
                <tr>
                    <th>作詞&nbsp;<span class="btn-danger">必須</span></th>
                    <td><input type="text" name="lyricist" placeholder="作詞" value="<?php echo set_value('lyricist', !empty($data->lyricist)?$data->lyricist:'') ?>" /></td>
                </tr>
                <tr>
                    <th>作曲&nbsp;<span class="btn-danger">必須</span></th>
                    <td><input type="text" name="composer" placeholder="作曲" value="<?php echo set_value('composer', !empty($data->composer)?$data->composer:'') ?>"></td>
                </tr>
                <tr>
                    <th>歌手&nbsp;<span class="btn-danger">必須</span></th>
                    <td><input type="text" name="singer" placeholder="歌手" value="<?php echo set_value('singer', !empty($data->singer)?$data->singer:'') ?>"></td>
                </tr>
                <tr>
                    <th>ジャンル&nbsp;<span class="btn-danger">必須</span></th>
                    <td><input type="text" name="genre" placeholder="ジャンル" value="<?php echo set_value('genre', !empty($data->genre)?$data->genre:'') ?>"></td>
                </tr>
                <tr>
                    <th>曲の長さ&nbsp;<span class="btn-danger">必須</span></th>
                    <td><input type="text" name="song_time" placeholder="曲の長さ" value="<?php echo set_value('song_time', !empty($data->song_time)?$data->song_time:'') ?>"></td>
                </tr>
                <tr>
                    <th>リリース日&nbsp;<span class="btn-danger">必須</span></th>
                    <td>
                        <div class="input-group">
                            <input id="datepicker1" class="input-sm"  name="release_date" type="text" placeholder="登録日" readonly value="<?php echo set_value('release_date', !empty($data->release_date)?$data->release_date:'') ?>"/>
                        </div>
                    </td>
                </tr>
            </table>
        </div>
        <div class="text-center" style="margin-bottom:10px;"><button type="submit" class="btn btn-success btn-lg">登録</button></div>
        </form>
    </div>
</div>
<?php
$this->load->view('common/footer');
